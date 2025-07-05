using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace Script.Module.ModuleScript
{
    // 立方体模块：在六个面各生成一个 ModuleSocket。
    [AddComponentMenu("Modules/CubeModule")]
    public class CubeModule : BaseModule
    {   
        [Header("插槽间距")]
        private float gap = 0.1f;
        
        [Header("插槽距离中心的半边长。如果留空则自动根据Collider计算。")]
        [SerializeField] private float socketOffset = -1f;          // -1 代表自动计算

        [Tooltip("插槽碰撞球半径")]
        [SerializeField] private float socketRadius = 0.01f;

        [Tooltip("插槽预制体")]
        [SerializeField] private GameObject socketVisualPrefab;

        // 六个朝向
        private static readonly Vector3[] _dirs =
        {
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right
        };
        
        protected override void CreateSockets()
        {
            // 计算插槽离中心的距离
            float offset = socketOffset;
            if (offset <= 0f) 
            {
                // 自动计算最大半边长，不加gap，让插槽刚好在表面
                var col = GetComponent<Collider>();
                offset = Mathf.Max(col.bounds.extents.x,
                    col.bounds.extents.y,
                    col.bounds.extents.z);
            }

            // 依次在 6 个面创建插槽子物体
            foreach (var dir in _dirs)
            {
                // 实例化prefab
                if (socketVisualPrefab != null)
                {
                    var socketGO = Instantiate(socketVisualPrefab);
                    socketGO.name = $"Socket_{dir}";
                    socketGO.transform.SetParent(transform, false);
                    socketGO.transform.localPosition = dir * offset;
                    socketGO.transform.localRotation = Quaternion.LookRotation(dir);
                    
                    // 设置层级
                    socketGO.layer = LayerMask.NameToLayer("Socket");
                    
                    // 获取ModuleSocket组件并设置
                    var moduleSocket = socketGO.GetComponent<ModuleSocket>();
                    if (moduleSocket != null)
                    {
                        moduleSocket.parentModule = this;
                        socketsList.Add(moduleSocket); // 重要：添加到插槽列表
                    }
                }
            }
        }
    }
}
