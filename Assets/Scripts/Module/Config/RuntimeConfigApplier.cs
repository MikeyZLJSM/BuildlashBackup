using Controllers;
using Module.Config;
using Module.Enums;
using UnityEngine;

namespace Module.Config
{
    /// <summary>
    /// 运行时配置应用器 - 用于在游戏运行时动态更新模块配置
    /// </summary>
    public class RuntimeConfigApplier : MonoBehaviour
    {
        [Header("配置应用设置")]
        [SerializeField] private bool autoApplyOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("快捷键设置")]
        [SerializeField] private KeyCode reloadConfigKey = KeyCode.F5;
        [SerializeField] private KeyCode applyToAllModulesKey = KeyCode.F6;
        
        private void Start()
        {
            if (autoApplyOnStart)
            {
                ApplyConfigToAllExistingModules();
            }
        }
        
        private void Update()
        {
            // 快捷键重新加载配置
            if (Input.GetKeyDown(reloadConfigKey))
            {
                ReloadAndApplyConfigs();
            }
            
            // 快捷键应用配置到所有模块
            if (Input.GetKeyDown(applyToAllModulesKey))
            {
                ApplyConfigToAllExistingModules();
            }
        }
        
        /// <summary>
        /// 重新加载配置并应用到所有模块
        /// </summary>
        public void ReloadAndApplyConfigs()
        {
            if (showDebugInfo)
                Debug.Log("重新加载Excel配置并应用到所有模块...");
                
            // 重新加载配置
            ModuleConfigManager.Instance.ReloadConfigs();
            
            // 应用到所有现有模块
            ApplyConfigToAllExistingModules();
            
            if (showDebugInfo)
                Debug.Log("配置重新加载完成");
        }
        
        /// <summary>
        /// 应用配置到场景中所有现有模块
        /// </summary>
        public void ApplyConfigToAllExistingModules()
        {
            BaseModule[] allModules = FindObjectsOfType<BaseModule>();
            int appliedCount = 0;
            
            foreach (BaseModule module in allModules)
            {
                try
                {
                    ModuleConfigManager.Instance.ApplyConfigToModule(module);
                    appliedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"应用配置到模块 {module.name} 失败: {e.Message}");
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"成功为 {appliedCount} 个模块应用了Excel配置");
        }
        
        /// <summary>
        /// 应用配置到指定类型的所有模块
        /// </summary>
        public void ApplyConfigToModuleType(ModuleType moduleType)
        {
            BaseModule[] allModules = FindObjectsOfType<BaseModule>();
            int appliedCount = 0;
            
            foreach (BaseModule module in allModules)
            {
                if (module.moduleType == moduleType)
                {
                    try
                    {
                        ModuleConfigManager.Instance.ApplyConfigToModule(module);
                        appliedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"应用配置到模块 {module.name} 失败: {e.Message}");
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"成功为 {appliedCount} 个 {moduleType} 类型模块应用了配置");
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label("Excel配置系统", GUI.skin.box);
            GUILayout.Label($"按 {reloadConfigKey} 重新加载配置");
            GUILayout.Label($"按 {applyToAllModulesKey} 应用配置到所有模块");
            GUILayout.EndArea();
        }
    }
}
