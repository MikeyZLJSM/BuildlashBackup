using System;
using Module;
using Module.Enums;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    ///     负责模块的选择逻辑，包括选中、取消选中、高亮显示等
    /// </summary>
    public class ModuleSelector : MonoBehaviour
    {
        [Header("模块选择设置")] [SerializeField] private Color selectedColor = Color.green;

        private Color _originalColor;

        private Renderer _selectedRenderer;

        public BaseModule SelectedModule { get; private set; }

        public bool HasSelection => SelectedModule != null;

        // 事件：当模块选择状态改变时触发
        public event Action<BaseModule> OnModuleSelected;
        public event Action<BaseModule> OnModuleDeselected;
        
        public static ModuleSelector Instance { get; private set; }
        public void Awake()
        {
            if (Instance is null) return;
            Instance = this;
        }
        private void SelectModule(BaseModule module)
        {
            if (module == null) return;
            if (!SelectedModule && module.moduleType == ModuleType.BaseCube) return; // 不能将BaseModule作为被拼接的模块
            if (SelectedModule && SelectedModule != module) return; // 限定一次只能选中一个模块

            SelectedModule = module;
            _selectedRenderer = module.GetComponent<Renderer>();

            if (_selectedRenderer != null)
            {
                _originalColor = _selectedRenderer.material.color;
                _selectedRenderer.material.color = selectedColor;
            }

            // 触发选择事件
            OnModuleSelected?.Invoke(SelectedModule);
        }

        public void DeselectModule()
        {
            if (SelectedModule == null) return;

            BaseModule previousModule = SelectedModule;

            if (_selectedRenderer != null) _selectedRenderer.material.color = _originalColor;

            // 清空选择
            SelectedModule = null;
            _selectedRenderer = null;

            //Debug.Log("取消模块选择");

            // 触发取消选择事件
            OnModuleDeselected?.Invoke(previousModule);
        }

        public bool ToggleModuleSelection(BaseModule module)
        {
            if (SelectedModule == module)
            {
                DeselectModule();
                return false;
            }

            SelectModule(module);
            return true;
        }

        public bool IsModuleSelected(BaseModule module)
        {
            return SelectedModule == module;
        }
    }
}