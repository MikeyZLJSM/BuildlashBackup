using System;
using UnityEngine;

namespace Scripts.Module
{
    /// <summary>
    /// 负责模块的选择逻辑，包括选中、取消选中、高亮显示等
    /// </summary>
    public class ModuleSelector : MonoBehaviour
    {
        [Header("模块选择设置")]
        [SerializeField] private Color selectedColor = Color.green;
        
        // 事件：当模块选择状态改变时触发
        public event Action<BaseModule> OnModuleSelected;
        public event Action<BaseModule> OnModuleDeselected;
        
        private BaseModule _selectedModule;
        private Color _originalColor;
        private Renderer _selectedRenderer;
        
        public BaseModule SelectedModule => _selectedModule;
        public bool HasSelection => _selectedModule != null;
        
        private void SelectModule(BaseModule module)
        {
            if (module == null) return;
            if(!_selectedModule && module.moduleType == ModuleType.BaseCube) return; // 不能将BaseModule作为被拼接的模块
            if(_selectedModule && _selectedModule != module) return; // 限定一次只能选中一个模块
            
            _selectedModule = module;
            _selectedRenderer = module.GetComponent<Renderer>();

            if (_selectedRenderer != null)
            {
                _originalColor = _selectedRenderer.material.color;
                _selectedRenderer.material.color = selectedColor;
            }

            Debug.Log($"选中模块: {_selectedModule.moduleType}");

            // 触发选择事件
            OnModuleSelected?.Invoke(_selectedModule);
        }
        
        public void DeselectModule()
        {
            if (_selectedModule == null) return;
            
            BaseModule previousModule = _selectedModule;
            
            if (_selectedRenderer != null)
            {
                _selectedRenderer.material.color = _originalColor;
            }
            
            // 清空选择
            _selectedModule = null;
            _selectedRenderer = null;
            
            Debug.Log("取消模块选择");
            
            // 触发取消选择事件
            OnModuleDeselected?.Invoke(previousModule);
        }
        
        public bool ToggleModuleSelection(BaseModule module)
        {
            if (_selectedModule == module)
            {
                DeselectModule();
                return false;
            }
            else
            {
                SelectModule(module);
                return true;
            }
        }
        
        public bool IsModuleSelected(BaseModule module)
        {
            return _selectedModule == module;
        }
    }
}