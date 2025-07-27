using System;
using System.Collections.Generic;
using Module;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;



namespace Controllers
{
    
    /// <summary> 模块管理，负责储存拼接情况和管理战斗逻辑 </summary>
    public partial class ModulesManager : MonoBehaviour
    {
        /// <summary> 中心模块 </summary>
        [Header("模块管理")] [SerializeField] private BaseModule centerModule ;
        /// <summary> 所有已拼装模块信息 </summary>
        [SerializeField] private List<ModuleInfo> assembledModules = new();
        /// <summary> 网格大小（每格的世界坐标单位 ） </summary>
        [Header("网格设置")] [SerializeField] private float gridSize = 1f;
        /// <summary> 敌人图层 </summary>
        [SerializeField] private LayerMask enemyLayerMask = 1 << 8; // 敌人图层
        /// <summary> 显示调试信息 </summary>
        [Header("调试信息")] [SerializeField] private bool showDebugInfo = true;
        
        
        
        public static ModulesManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance is not null) return;
                Instance = this;
                
        }
        
        private void Start()
        {
            centerModule = GameObject.Find("BaseCube")?.GetComponent<BaseModule>();
            // 确保中心模块已设置
            if (centerModule == null) Debug.LogError("ModulesManager 中心模块未设置");

            // 初始化模块
            InitializeModules();
        }
        

        /// <summary> 初始化时收集所有现有模块 </summary>
        private void InitializeModules()
        {
            assembledModules.Clear();

            if (centerModule != null)
            {
                RegisterModule(centerModule);
                CollectChildModules(centerModule);
            }

            if (showDebugInfo) Debug.Log($"[ModulesManager] 初始化完成，收集到 {assembledModules.Count} 个模块");
        }

        /// <summary> 注册新模块（在拼接时调用） </summary>
        /// <param name = "module" > 要注册的模块 </param>
        /// <param name = "customName" > 自定义名称 </param>
        public void RegisterModule(BaseModule module, string customName = "")
        {
            if (module == null || ContainsModule(module)) return;


            // 使用世界坐标进行计算
            Vector3 centerPos = centerModule != null ? centerModule.transform.position : transform.position;
            ModuleInfo moduleInfo = new(module, centerPos, gridSize, customName);
            assembledModules.Add(moduleInfo);

            if (showDebugInfo)
                Debug.Log(
                    $"[ModulesManager] 模块 {moduleInfo.moduleName} 已注册，网格偏移: ({moduleInfo.gridOffset.x}, {moduleInfo.gridOffset.y}, {moduleInfo.gridOffset.z})");
        }

        /// <summary> 注销模块（在分离时调用） </summary>
        /// <param name = "module" > 要注销的模块 </param>
        public void UnregisterModule(BaseModule module)
        {
            int removedCount = assembledModules.RemoveAll(info => info.module == module);

            if (showDebugInfo && removedCount > 0) Debug.Log($"[ModulesManager] 模块 {module.name} 已注销");
        }

        /// <summary> 检查是否包含指定模块 </summary>
        /// <param name = "module" > 要检查的模块 </param>
        /// <returns> 是否包含该模块 </returns>
        public bool ContainsModule(BaseModule module)
        {
            return assembledModules.Exists(info => info.module == module && info.module != null);
        }

        /// <summary> 递归收集子模块 </summary>
        /// <param name = "parentModule" > 父模块 </param>
        private void CollectChildModules(BaseModule parentModule)
        {
            if (parentModule?._childModules == null) return;

            foreach (BaseModule childModule in parentModule._childModules)
                if (childModule != null)
                {
                    RegisterModule(childModule);
                    CollectChildModules(childModule); // 递归收集子模块的子模块
                }
        }

        /// <summary> 获取所有模块列表 </summary>
        /// <returns> 模块信息列表 </returns>
        public List<ModuleInfo> GetAllModulesInfo()
        {
            return new List<ModuleInfo>(assembledModules);
        }

        /// <summary> 获取指定类型的模块 </summary>
        /// <typeparam name = "T" > 模块类型 </typeparam>
        /// <returns> 指定类型的模块列表 </returns>
        public List<T> GetModulesOfType<T>() where T : BaseModule
        {
            List<T> result = new();
            foreach (ModuleInfo moduleInfo in assembledModules)
                if (moduleInfo.module is T module)
                    result.Add(module);

            return result;
        }

        /// <summary> 根据网格偏移获取模块 </summary>
        /// <param name = "gridOffset" > 网格偏移量 </param>
        /// <returns> 匹配的模块 </returns>
        public BaseModule GetModuleByGridOffset(Vector3Int gridOffset)
        {
            foreach (ModuleInfo moduleInfo in assembledModules)
                if (moduleInfo.gridOffset == gridOffset)
                    return moduleInfo.module;

            return null;
        }

        /// <summary> 根据网格坐标获取模块信息 </summary>
        /// <param name = "gridOffset" > 网格偏移量 </param>
        /// <returns> 模块信息 </returns>
        public ModuleInfo GetModuleInfoByGridOffset(Vector3Int gridOffset)
        {
            foreach (ModuleInfo moduleInfo in assembledModules)
                if (moduleInfo.gridOffset == gridOffset)
                    return moduleInfo;

            return null;
        }

        /// <summary> 根据模块获取模块信息 </summary>
        /// <param name = "module" > 模块 </param>
        /// <returns> 模块信息 </returns>
        public ModuleInfo GetModuleInfoByModule(BaseModule module)
        {
            foreach (ModuleInfo moduleInfo in assembledModules)
                if (moduleInfo.module == module)
                    return moduleInfo;

            return null;
        }

        /// <summary> 检查指定网格位置是否有模块 </summary>
        /// <param name = "gridOffset" > 网格偏移量 </param>
        /// <returns> 是否有模块 </returns>
        public bool HasModuleAtGridPosition(Vector3Int gridOffset)
        {
            return GetModuleByGridOffset(gridOffset) != null;
        }

        /// <summary> 获取所有模块的网格边界 </summary>
        /// <returns> 最小和最大网格坐标 </returns>
        public (Vector3Int min, Vector3Int max) GetGridBounds()
        {
            if (assembledModules.Count == 0)
                return (Vector3Int.zero, Vector3Int.zero);

            Vector3Int min = assembledModules[0].gridOffset;
            Vector3Int max = assembledModules[0].gridOffset;

            foreach (ModuleInfo moduleInfo in assembledModules)
            {
                min = Vector3Int.Min(min, moduleInfo.gridOffset);
                max = Vector3Int.Max(max, moduleInfo.gridOffset);
            }

            return (min, max);
        }

        /// <summary> 根据相对偏移获取模块（保留用于兼容性） </summary>
        /// <param name = "offset" > 相对偏移 </param>
        /// <param name = "tolerance" > 容差 </param>
        /// <returns> 匹配的模块 </returns>
        public BaseModule GetModuleByOffset(Vector3 offset, float tolerance = 0.1f)
        {
            foreach (ModuleInfo moduleInfo in assembledModules)
                if (Vector3.Distance(moduleInfo.relativePosition, offset) <= tolerance)
                    return moduleInfo.module;

            return null;
        }

        /// <summary> 设置中心模块 </summary>
        /// <param name = "module" > 新的中心模块 </param>
        public void SetCenterModule(BaseModule module)
        {
            centerModule = module;
            InitializeModules(); // 重新初始化所有模块
        }

        /// <summary> 获取中心模块 </summary>
        /// <returns> 中心模块 </returns>
        public BaseModule GetCenterModule()
        {
            return centerModule;
        }

        /// <summary> 当模块拼接时调用此方法（由BaseModule调用） </summary>
        /// <param name = "parentModule" > 父模块 </param>
        /// <param name = "childModule" > 子模块 </param>
        public void OnModuleAttached(BaseModule parentModule, BaseModule childModule)
        {
            RegisterModule(childModule);
        }

        /// <summary> 当模块分离时调用此方法（由BaseModule调用） </summary>
        /// <param name = "module" > 被分离的模块 </param>
        public void OnModuleDetached(BaseModule module)
        {
            UnregisterModule(module);
        }
        

        /// <summary> 打印所有模块信息（调试用） </summary>
        [ContextMenu("打印模块信息")]
        public void PrintModulesInfo()
        {
            Debug.Log("=== ModulesManager 模块信息 ===");
            Debug.Log($"中心模块: {(centerModule != null ? centerModule.name : "无")}");
            Debug.Log($"网格大小: {gridSize}");
            Debug.Log($"总模块数: {assembledModules.Count}");

            (Vector3Int min, Vector3Int max) bounds = GetGridBounds();
            Debug.Log(
                $"网格边界: 最小({bounds.min.x}, {bounds.min.y}, {bounds.min.z}) 最大({bounds.max.x}, {bounds.max.y}, {bounds.max.z})");

            foreach (ModuleInfo moduleInfo in assembledModules)
                Debug.Log($"模块: {moduleInfo.moduleName}, " +
                          $"网格偏移: ({moduleInfo.gridOffset.x}, {moduleInfo.gridOffset.y}, {moduleInfo.gridOffset.z}), " +
                          $"世界偏移: ({moduleInfo.relativePosition.x:F1}, {moduleInfo.relativePosition.y:F1}, {moduleInfo.relativePosition.z:F1}), " +
                          $"攻击模块: {moduleInfo.isAttackModule}");
        }

        /// <summary> 打印网格布局（调试用） </summary>
        [ContextMenu("打印网格布局")]
        public void PrintGridLayout()
        {
            (Vector3Int min, Vector3Int max) bounds = GetGridBounds();
            Debug.Log($"=== 网格布局 (Y={bounds.min.y} 层) ===");

            // 打印一个水平层的网格布局
            for (int z = bounds.max.z; z >= bounds.min.z; z--)
            {
                string line = "";
                for (int x = bounds.min.x; x <= bounds.max.x; x++)
                {
                    BaseModule moduleAtPos = GetModuleByGridOffset(new Vector3Int(x, bounds.min.y, z));
                    if (moduleAtPos != null)
                        line += "[M] ";
                    else
                        line += "[ ] ";
                }

                Debug.Log($"Z={z}: {line}");
            }
        }


        /// <summary> 储存模块信息的类 </summary>
        [Serializable]
        public class ModuleInfo
        {
            public BaseModule module;
            /// <summary> 模块相对于中心模块的网格偏移量(按格子计算) </summary>
            /// <example> (1,0,-1) </example>
            public Vector3Int gridOffset;
            public Vector3 relativePosition; // 世界坐标相对位置（用于调试）
            public string moduleName;
            public bool isAttackModule; // 是否为攻击模块


            /// <param name = "module" > 模组对象 </param>
            /// <param name = "centerPosition" > 中心模块的位置 </param>
            /// <param name = "gridSize" > 网格大小 </param>
            /// <param name = "name" > 模块名称 </param>
            public ModuleInfo(BaseModule module, Vector3 centerPosition, float gridSize, string name = "")
            {
                this.module = module;
                moduleName = string.IsNullOrEmpty(name) ? module.name : name;
                relativePosition = module.transform.position - centerPosition;

                // 计算网格偏移量：将世界坐标差值转换为网格单位
                gridOffset = new Vector3Int(
                    Mathf.RoundToInt(relativePosition.x / gridSize),
                    Mathf.RoundToInt(relativePosition.y / gridSize),
                    Mathf.RoundToInt(relativePosition.z / gridSize)
                );

                // 检查模块是否具有IAttackable接口
                isAttackModule = module.GetComponent<BaseAttackModule>();
            }
        }
    }
}