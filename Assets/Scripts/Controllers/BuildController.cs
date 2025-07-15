using System;
using Module;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    ///     建造控制器，专注于模块的拼接和拆除逻辑
    /// </summary>
    public class BuildController : MonoBehaviour
    {
        public static BuildController Instance;

        [Header("游戏摄像头")] public Camera gameCamera;

        [SerializeField] private ModuleSelector _moduleSelector;

        

        [SerializeField] [Header("模块的层级")] private int moduleLayer = 7;

        [Header("删除键")] [SerializeField] private KeyCode removeButton = KeyCode.E;



        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            HandleMouseInput();
        }


        /// <summary>
        ///     处理鼠标输入
        /// </summary>
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.GetKey(removeButton))
                    // 删除模式
                    HandleRemoveInput();
                else
                    // 正常建造模式
                    HandleBuildInput();
            }
        }

        /// <summary>
        ///     处理删除输入
        /// </summary>
        private void HandleRemoveInput()
        {
            if (TryClickModule(out var module)) RemoveModule(module);
        }

        /// <summary>
        ///     处理建造输入
        /// </summary>
        private void HandleBuildInput()
        {
            if (TryClickModule(out var clickedModule))
            {
                _moduleSelector.ToggleModuleSelection(clickedModule);

                if (_moduleSelector.HasSelection) TryAssembleModule(_moduleSelector.SelectedModule, clickedModule);
            }
        }


        private bool TryClickModule(out BaseModule module)
        {
            Debug.Log("尝试点击模块");
            module = null;

            // 射线检测 Module 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out var hit, 100f,
                    1 << moduleLayer, QueryTriggerInteraction.Collide))
            {
                Debug.Log("没有点击到模块!");
                return false;
            }

            module = hit.collider.GetComponent<BaseModule>();
            return module;
        }


        private void RemoveModule(BaseModule targetModule)
        {
            // 如果要删除的是当前选中的模块，先取消选择
            if (_moduleSelector.IsModuleSelected(targetModule)) _moduleSelector.DeselectModule();
            
            targetModule.RemoveModule();
        }

        /// <summary>
        ///     模块对模块拼接（支持所有实现 IAttachable 的模块）
        /// </summary>
        private void TryAssembleModule(BaseModule fromModule, BaseModule toModule)
        {
            if (toModule.moduleType != ModuleType.BaseCube && !toModule.parentModule) return; // 禁止单独的子模块拼接

            if (fromModule is IAttachable fromAttach && toModule is IAttachable toAttach)
            {
                var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f, 1 << moduleLayer, QueryTriggerInteraction.Collide))
                {
                    var targetNormal = hit.normal; // 父模块被点击面的法线

                    // 计算父模块被点击面的中心点
                    var faces = toAttach.GetAttachableFaces();
                    var bestIdx = 0;
                    var bestDot = -1f;

                    for (var i = 0; i < faces.Length; i++)
                    {
                        var dot = Vector3.Dot(faces[i].normal, targetNormal);
                        if (dot > bestDot)
                        {
                            bestDot = dot;
                            bestIdx = i;
                        }
                    }

                    var targetFaceCenter = faces[bestIdx].center;
                    var ok = fromAttach.AttachToFace(toModule, targetNormal, targetFaceCenter, hit.point);

                    if (ok)
                    {
                        _moduleSelector.DeselectModule();
                        Debug.Log("模块拼接成功!");
                    }
                    else
                    {
                        Debug.Log("模块拼接失败!");
                    }
                }
            }
        }


        // ------------------------------以下方法已弃用---------------------------------------------
    
        private ModuleSocket _selectedChildSocket;
        [SerializeField] [Header("插槽的层级")] private int socketLayer = 8;

        // 插槽选择相关
        public bool IsActive => _selectedChildSocket != null;
        
        // <summary>
        /// 插槽拼接方式（目前不使用）
        // </summary>
        [Obsolete("请使用新的模块对模块拼接方法")]
        private void TryAssemble(ModuleSocket parentSocket)
        {
            // 父插槽必须空
            if (parentSocket.IsAttached) return;
            if (_selectedChildSocket == null) return;

            // 尝试链接
            var ok = parentSocket.parentModule
                .AttachChildModule(_selectedChildSocket.parentModule, parentSocket, _selectedChildSocket);

            // 如果拼装成功
            if (ok)
            {
                Debug.Log("拼接成功!");
                CancelSocketSelection();
                // 取消模块选择
                _moduleSelector.DeselectModule();
                // 更新父插槽颜色
                parentSocket.GetComponent<SocketSelector>().SetNormal();
            }
            else
            {
                Debug.Log("拼接失败!");
            }
        }


        /// <summary>
        ///     拆除模块（插槽连接方式） 目前不使用
        /// </summary>
        [Obsolete("请使用新的模块对模块拼接方法")]
        private void TryRemoveModule(BaseModule module)
        {
            if (module.parentModule == null) return;

            var parentModule = module.parentModule;
            var parentSocket = parentModule.FindSocketAttachedToModule(module);
            var childSocket = module.FindSocketAttachedToModule(parentModule);

            parentModule.RemoveChildModule(module);
            parentSocket.Detach();
            childSocket.Detach();
        }


        /// <summary>
        ///     点击插槽 目前不使用
        // </summary>
        [Obsolete("请使用新的模块对模块拼接方法")]
        private bool TryClickSocket(out ModuleSocket socket)
        {
            Debug.Log("尝试点击插槽...");
            socket = null;

            // 射线检测 Socket 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out var hit, 100f,
                    1 << socketLayer, QueryTriggerInteraction.Collide))
            {
                Debug.Log("没有点击到插槽!");
                return false;
            }

            // 尝试取出组件
            socket = hit.collider.GetComponent<ModuleSocket>();
            return socket != null;
        }

        [Obsolete("请使用新的模块对模块拼接方法")]
        public void CancelSocketSelection()
        {
            if (_selectedChildSocket != null)
            {
                var socketSelector = _selectedChildSocket.GetComponent<SocketSelector>();
                if (socketSelector != null)
                    socketSelector.SetNormal();
            }

            _selectedChildSocket = null;
            Debug.Log("取消插槽选择");
        }

        [Obsolete("请使用新的模块对模块拼接方法")]
        public void SelectChildSocket(ModuleSocket childSocket)
        {
            if (childSocket == null || childSocket.IsAttached) return;

            // 如果之前已经选择了插槽，先取消高亮
            if (_selectedChildSocket != null)
            {
                var prevSelector = _selectedChildSocket.GetComponent<SocketSelector>();
                if (prevSelector != null)
                    prevSelector.SetNormal();
            }

            _selectedChildSocket = childSocket;

            // 高亮当前选中的插槽
            var socketSelector = _selectedChildSocket.GetComponent<SocketSelector>();
            if (socketSelector != null) socketSelector.SetPicked();

            Debug.Log("选中子插槽: " + _selectedChildSocket.name);

        }
        public ModuleSocket CurrentChildSocket()
        {
            return _selectedChildSocket;
        }

    }
    
    
    
}