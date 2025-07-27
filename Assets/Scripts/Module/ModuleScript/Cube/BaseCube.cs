using Module.Enums;
using UnityEngine;

namespace Module.ModuleScript.Cube
{
    ///<summary>基础立方体模块 - 确保场景中只能存在一个实例</summary>
    [AddComponentMenu("Modules/CubeModule")]
    public sealed class BaseCube : NormalCube
    {
        private static BaseCube _instance;
        
        ///<summary>获取BaseCube的单例实例</summary>
        public static BaseCube Instance => _instance;
        
        protected override void Awake()
        {
            // 检测是否已经存在BaseCube实例
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            base.Awake();
            DontDestroyOnLoad(gameObject);
            _rb.isKinematic = true;
            _rb.detectCollisions = true;
            moduleType = ModuleType.BaseCube;
        }
        
        private void OnDestroy()
        {
            // 当实例被销毁时清理静态引用
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
