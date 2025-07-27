using UnityEngine;
using System.Collections;

namespace Module.Battle
{
    /// <summary>
    /// 溅射伤害的视觉效果控制器
    /// </summary>
    public class SplashVisualEffect : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _expandSpeed = 2.0f;
        [SerializeField] private float _maxScale = 1.0f;
        [SerializeField] private Color _startColor = new Color(1f, 0.5f, 0f, 0.7f);
        [SerializeField] private Color _endColor = new Color(1f, 0.5f, 0f, 0f);
        
        private Renderer _renderer;
        private Material _material;
        private float _startTime;
        private float _initialScale;
        
        /// <summary>
        /// 初始化溅射效果
        /// </summary>
        /// <param name="radius">溅射半径</param>
        /// <param name="duration">持续时间</param>
        public void Initialize(float radius, float duration = 0.5f)
        {
            _duration = duration;
            _maxScale = radius * 2;
            _initialScale = _maxScale * 0.2f; // 起始大小为最大大小的20%
            transform.localScale = new Vector3(_initialScale, _initialScale, _initialScale);
            _startTime = Time.time;
            
            StartCoroutine(AnimateEffect());
        }
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer)
            {
                // 创建新材质
                _material = new Material(Shader.Find("Standard"));
                _material.SetFloat("_Mode", 3); // 设置为透明模式
                _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _material.SetInt("_ZWrite", 0);
                _material.DisableKeyword("_ALPHATEST_ON");
                _material.EnableKeyword("_ALPHABLEND_ON");
                _material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                _material.renderQueue = 3000;
                _renderer.material = _material;
            }
            
            // 禁用碰撞器
            Collider collider = GetComponent<Collider>();
            if (collider)
            {
                collider.enabled = false;
            }
        }
        
        /// <summary>
        /// 动画效果协程
        /// </summary>
        private IEnumerator AnimateEffect()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < _duration)
            {
                elapsedTime = Time.time - _startTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / _duration);
                
                // 计算当前缩放
                float currentScale = Mathf.Lerp(_initialScale, _maxScale, normalizedTime);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                
                // 计算当前颜色
                if (_material)
                {
                    _material.color = Color.Lerp(_startColor, _endColor, normalizedTime);
                }
                
                yield return null;
            }
            
            // 动画结束后销毁对象
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 在指定位置创建溅射效果
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="radius">半径</param>
        /// <param name="duration">持续时间</param>
        /// <returns>创建的效果对象</returns>
        public static SplashVisualEffect Create(Vector3 position, float radius, float duration = 0.5f)
        {
            GameObject effectObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effectObj.name = "SplashEffect";
            effectObj.transform.position = position;
            
            SplashVisualEffect effect = effectObj.AddComponent<SplashVisualEffect>();
            effect.Initialize(radius, duration);
            
            return effect;
        }
    }
}