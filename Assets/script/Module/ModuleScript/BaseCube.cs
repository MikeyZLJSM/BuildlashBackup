using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UIElements;

namespace Script.Module.ModuleScript
{
    // 基础立方体模块
    [AddComponentMenu("Modules/CubeModule")]
    public class BaseCube : BaseModule
    {   
        [Header("插槽间距")]
        private float gap = 0.1f;
        
        [Header("插槽距离中心的半边长。如果留空则自动根据Collider计算。")]
        [SerializeField] private float socketOffset = -1f;          // -1 代表自动计算

        [Tooltip("插槽碰撞球半径")]
        [SerializeField] private float socketRadius = 0.01f;

        [Tooltip("插槽预制体")]
        [SerializeField] private GameObject socketVisualPrefab;

        // 六个面：forward + up 配对
        private (Vector3 forward, Vector3 up)[] faces = new (Vector3 forward, Vector3 up)[]
        {
            (Vector3.up,     Vector3.back),    // 上
            (Vector3.down,   Vector3.forward), // 下
            (Vector3.forward,Vector3.up),      // 前
            (Vector3.back,   Vector3.up),      // 后
            (Vector3.right,  Vector3.up),      // 右
            (Vector3.left,   Vector3.up)       // 左
        };
        
        protected override void CreateSockets()
        {
            // 计算插槽离中心的距离
            Vector3 offset = GetComponent<BoxCollider>().size * 0.5f;
            if (socketOffset > 0f)
            {
                offset = Vector3.one * socketOffset;
            }
            

            // 依次在 6 个面创建插槽子物体
            foreach (var (dir,up) in faces)
            {
                // 实例化prefab
                if (socketVisualPrefab != null)
                {
                    var socketGO = Instantiate(socketVisualPrefab,transform,false);
                    socketGO.name = $"Socket_{dir}";

                    Vector3 pos = new Vector3(
                        dir.x * (offset.x + socketRadius + gap),
                        dir.y * (offset.y + socketRadius + gap),
                        dir.z * (offset.z + socketRadius + gap));
                    socketGO.transform.localPosition = pos;
                    
                    // 设置层级
                    socketGO.transform.localRotation = Quaternion.LookRotation(dir, up);
                    
                    // 获取ModuleSocket组件并设置
                    var moduleSocket = socketGO.GetComponent<ModuleSocket>();
                    if (moduleSocket != null)
                    {
                        moduleSocket.parentModule = this;
                        socketsList.Add(moduleSocket); // 添加到插槽列表
                    }
                }
            }
        }
    }
}
