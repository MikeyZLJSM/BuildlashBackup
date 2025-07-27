using Module;
using Module.Battle;
using UnityEngine;
using Module.Config;

namespace Controllers
{
    ///<summary>模块配置管理器 - 单例模式</summary>
    public class ModuleConfigManager : MonoBehaviour
    {
        [Header("配置表设置")]
        [Header("Excel表格名称")]
        public static string SheetName = "配置表"; // Excel表格名称
        [Header("配置文件路径")]
        public static string ConfigFilePath = "/Config/数值表.xlsx"; 
        
        public static ModuleConfigManager Instance { get; private set; }

        private ExcelConfigReader _excelReader = new ExcelConfigReader(ConfigFilePath, SheetName);
        
        private bool _isInitialized;
        
        private void Awake()
        {
            // 确保单例模式的正确实现
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("发现多个ModuleConfigManager实例，销毁重复实例");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
 
        }
        private void Start()
        {
            // 初始化配置
            InitializeConfigs();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        ///<summary>初始化配置</summary>
        private void InitializeConfigs()
        {
            if (_isInitialized)
            {
                return;
            }
            
            try
            {
                
                if (_excelReader != null)
                {
                    // 加载Excel配置
                    _excelReader.LoadConfigs();
                    _isInitialized = true;
                    Debug.Log("模块配置管理器初始化完成");
                }
                else
                {
                    Debug.LogError("无法创建ExcelConfigReader组件");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"配置初始化失败: {ex.Message}");
            }
        }
        
        
        ///<summary>根据模块类名获取配置</summary>
        public ModuleParameters GetModuleConfig(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                Debug.LogWarning("模块名称为空，无法获取配置");
                return null;
            }
            
            if (!EnsureInitialized())
            {
                return null;
            }
            
            return _excelReader.GetModuleConfig(moduleName);
        }
        
        ///<summary>根据模块对象获取配置</summary>
        public ModuleParameters GetModuleConfig(BaseModule module)
        {
            if (module == null)
            {
                Debug.LogWarning("模块对象为null，无法获取配置");
                return null;
            }
            
            if (!EnsureInitialized())
            {
                return null;
            }
            
            return _excelReader.GetModuleConfig(module);
        }
        
        ///<summary>应用配置到模块</summary>
        public bool ApplyConfigToModule(BaseModule module)
        {
            if (module == null)
            {
                Debug.LogWarning("模块对象为null，无法应用配置");
                return false;
            }
            
            ModuleParameters config = GetModuleConfig(module);
            if (config != null)
            {
                // 由于BaseModule可能没有统一的ApplyConfig方法
                // 这里返回true表示成功获取到配置，具体应用由调用者处理
                Debug.Log($"成功获取模块配置: {module.GetType().Name}");
                return true;
            }
            else
            {
                Debug.LogWarning($"未找到模块 {module.GetType().Name} 的配置");
                return false;
            }
        }
        
        ///<summary>确保管理器已初始化</summary>
        private bool EnsureInitialized()
        {
            if (!_isInitialized)
            {
                InitializeConfigs();
            }
            
            if (_excelReader == null)
            {
                Debug.LogError("ExcelReader为null，无法获取配置");
                return false;
            }
            
            return _isInitialized;
        }
        
        ///<summary>重新加载配置</summary>
        public void ReloadConfigs()
        {
            _isInitialized = false;
            InitializeConfigs();
        }
        
        ///<summary>检查配置是否已初始化</summary>
        public bool IsInitialized => _isInitialized;
    }
}