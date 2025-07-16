using System.Runtime.CompilerServices;
using System;
using Module;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// 建造控制器，专注于模块的拼接和拆除逻辑
    /// </summary>
    public class BuildController : MonoBehaviour
    {
        public static BuildController Instance;

        [Header("游戏摄像头")] public Camera gameCamera;

        [SerializeField] private ModuleSelector _moduleSelector;
        
        [SerializeField, Header("插槽的层级")] 
        private int socketLayer = 8;
        
        [SerializeField, Header("模块的层级")] 
        private int moduleLayer = 7;    
        
        [Header("删除键"), SerializeField] 
        private KeyCode removeButton = KeyCode.E;

        [Header("拼接预览")]
        private GameObject _previewObject;
        private Material _previewMaterial;
        private bool _showingPreview = false;
        
        // 插槽选择相关
        public bool IsActive => _selectedChildSocket != null;
        private ModuleSocket _selectedChildSocket;

        private void Awake()
        {
            if (Instance != null) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _previewMaterial = new Material(Shader.Find("Standard"));
            _previewMaterial.color = new Color(1, 1, 1, 0.5f); // 半透明白色
            _previewMaterial.SetFloat("_Mode", 3); // 透明模式
            _previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _previewMaterial.SetInt("_ZWrite", 0);
            _previewMaterial.DisableKeyword("_ALPHATEST_ON");
            _previewMaterial.EnableKeyword("_ALPHABLEND_ON");
            _previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _previewMaterial.renderQueue = 3000;
        }

        #region sockets
        
        public ModuleSocket CurrentChildSocket()
        {
            return _selectedChildSocket;
        }
        
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
            if (socketSelector != null)
            {
                socketSelector.SetPicked();
            }

            Debug.Log("选中子插槽: " + _selectedChildSocket.name);
        }
        
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
        
        private void TryAssemble(ModuleSocket parentSocket)
        {
            // 父插槽必须空
            if (parentSocket.IsAttached) return;
            if (_selectedChildSocket == null) return;

            // 尝试链接
            bool ok = parentSocket.parentModule
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
        /// 拆除模块（插槽连接方式）
        /// </summary>
        private void TryRemoveModule(BaseModule module)
        {
            if (module.parentModule == null) return;

            BaseModule parentModule = module.parentModule;
            ModuleSocket parentSocket = parentModule.FindSocketAttachedToModule(module);
            ModuleSocket childSocket = module.FindSocketAttachedToModule(parentModule);
            
            parentModule.RemoveChildModule(module);
            parentSocket.Detach();
            childSocket.Detach();
        }
        
        private bool TryClickSocket(out ModuleSocket socket)
        {
            Debug.Log("尝试点击插槽...");
            socket = null;

            // 射线检测 Socket 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit, 100f,
                    1 << socketLayer, QueryTriggerInteraction.Collide))
            {
                Debug.Log("没有点击到插槽!");
                return false;
            }

            // 尝试取出组件
            socket = hit.collider.GetComponent<ModuleSocket>();
            return socket != null;
        }
        
        #endregion

        private void Update()
        {
            HandleMouseInput();
        }
        
        private void HandleMouseInput()
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, 1 << moduleLayer))
            {
                if (_moduleSelector.SelectedModule && !Input.GetKey(removeButton) && !_moduleSelector.SelectedModule.parentModule)
                {
                    BaseModule targetModule = hit.collider.GetComponent<BaseModule>();
                    
                    //若目前没有显示拼接预览，或者拼接预览有变动，则进入TryPreviewAssemble
                    if (!_showingPreview || ((_lastTagetModule != targetModule) || (_lastTagetModule == targetModule && _lastTargetNormal != hit.normal)))
                    {
                        if (!targetModule) return;
                        if (targetModule != _moduleSelector.SelectedModule && targetModule.CanBeAttachedTarget())
                        {
                            TryPreviewAssemble(hit, targetModule);
                        }
                    }
                }
            }
            else if (_showingPreview)
            {
                HidePreview();
                _lastTargetNormal = Vector3.zero;
                _lastTagetModule = null;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (_showingPreview)
                {
                    CompleteAssemble();
                }
                else if (Input.GetKey(removeButton))
                {
                    // 删除模式
                    HandleRemoveInput();
                }
                else
                {
                    // 正常建造模式
                    HandleBuildInput();
                }
            }
        }

        /// <summary>
        /// 处理删除输入
        /// </summary>
        private void HandleRemoveInput()
        {
            if (TryClickModule(out BaseModule module))
            {
                RemoveModule(module);
            }
        }

        /// <summary>
        /// 处理建造输入
        /// </summary>
        private void HandleBuildInput()
        {
            if (TryClickModule(out BaseModule clickedModule))
            {
                _moduleSelector.ToggleModuleSelection(clickedModule);
                
                if (_moduleSelector.HasSelection)
                {
                    TryAssembleModule(_moduleSelector.SelectedModule, clickedModule);
                }
            }
        }
        
        private bool TryClickModule(out BaseModule module)
        {
            //Debug.Log("尝试点击模块");
            module = null;
            
            // 射线检测 Module 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit, 100f,
                    1 << moduleLayer, QueryTriggerInteraction.Collide))
            {
                //Debug.Log("没有点击到模块!");
                return false;
            }
            
            module = hit.collider.GetComponent<BaseModule>();
            return module;
        }
        
        private void RemoveModule(BaseModule targetModule)
        {
            // 如果要删除的是当前选中的模块，先取消选择
            if (_moduleSelector.IsModuleSelected(targetModule))
            {
                _moduleSelector.DeselectModule();
            }
            
            targetModule.RemoveModule();
        }

        private Vector3 _lastTargetNormal;
        private BaseModule _lastTagetModule;

        private void CreateOrUpdatePreview(BaseModule sourceModule, BaseModule targetModule, Vector3 targetNormal,
            Vector3 targetFaceCenter, Vector3 hitPoint)
        {
            if (targetModule == _lastTagetModule && targetNormal != _lastTargetNormal
                || targetModule != _lastTagetModule && targetNormal == _lastTargetNormal)
            {
                HidePreview();
            }
            
            _lastTagetModule = targetModule;
            _lastTargetNormal = targetNormal;

            _previewObject = Instantiate(sourceModule.gameObject);
            BaseModule previewModule = _previewObject.GetComponent<BaseModule>();
            previewModule.SetPhysicsAttached(true);

            // 应用半透明材质
            Renderer[] renderers = _previewObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = _previewMaterial;
                }

                renderer.materials = materials;
            }

            previewModule.AttachToFace(targetModule, targetNormal.normalized, targetFaceCenter, hitPoint);
            previewModule.gameObject.layer = 0;
            _showingPreview = true;
        }
        
        // 隐藏预览
        private void HidePreview()
        {
            if (_previewObject)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }
            _showingPreview = false;
        }
    
        // 完成拼接
        private void CompleteAssemble()
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << moduleLayer, QueryTriggerInteraction.Collide))
            {
                BaseModule targetModule = hit.collider.GetComponent<BaseModule>();
                if (targetModule != null && targetModule != _moduleSelector.SelectedModule)
                {
                    TryAssembleModule(_moduleSelector.SelectedModule, targetModule);
                }
            }
        
            HidePreview();
        }

        /// <summary>
        /// 模块对模块拼接（支持所有实现 IAttachable 的模块）
        /// </summary>
        private void TryAssembleModule(BaseModule fromModule, BaseModule targetModule)
        {
            if (targetModule.moduleType != ModuleType.BaseCube && !targetModule.parentModule) return; // 禁止单独的子模块拼接
            
            if (fromModule is IAttachable fromAttach && targetModule is IAttachable targetToAttach)
            {
                Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << moduleLayer, QueryTriggerInteraction.Collide))
                {
                    Vector3 targetNormal = hit.normal; // 父模块被点击面的法线
                    var faces = targetToAttach.GetAttachableFaces();
                    
                    bool hitIntoAttachableFace = false;
                    foreach (var face in faces)
                    {
                        if(targetNormal != face.normal) continue;
                        hitIntoAttachableFace = true;
                        break;
                    }
                    if(hitIntoAttachableFace == false) return;
                    
                    int bestIdx = FindBestAttachableFaceIndex(faces, targetNormal, targetModule);
                    if (bestIdx < 0) return;
                    Vector3 targetFaceCenter = faces[bestIdx].center;
                    bool ok = fromAttach.AttachToFace(targetModule, targetNormal.normalized, targetFaceCenter, hit.point);
                    if (ok)
                    {
                        _moduleSelector.DeselectModule();
                    }

                }
            }
        }
        
        private int FindBestAttachableFaceIndex((Vector3 normal, Vector3 center, bool canAttach)[] faces,
            Vector3 targetNormal, BaseModule targetModule)
        {
            ModuleType targetType = targetModule.moduleType;

            int bestIdx = -1;
            float bestValue = -1f;
            
            switch (targetType)
            {
                case ModuleType.BaseCube: 
                case ModuleType.NormalCube:
                case ModuleType.NormalCylinder:
                    for (int i = 0; i < faces.Length; i++)
                    {
                        if (!faces[i].canAttach) continue;
                        float value = Vector3.Dot(faces[i].normal, targetNormal);

                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestIdx = i;
                        }
                    }
                    break;
                
            }
            
            return bestIdx;
        }

        // 查找目标模块上与射线命中点最匹配的可拼接面
        private bool TryFindTargetAttachableFace(RaycastHit hit, BaseModule targetModule, 
                                         out Vector3 targetFaceNormal, out Vector3 targetFaceCenter)
        {
            targetFaceNormal = Vector3.zero;
            targetFaceCenter = Vector3.zero;
            var targetAttach = targetModule as IAttachable;
            if (targetAttach == null)
                return false;
            Vector3 hitNormal = hit.normal;
            
            var targetFaces = targetAttach.GetAttachableFaces();
            
            //TODO:提取hit中指定面逻辑
            bool hitIntoAttachableFace = false;
            foreach (var face in targetFaces)
            {
                if(hitNormal != face.normal) continue;
                hitIntoAttachableFace = true;
                break;
            }
            if(hitIntoAttachableFace == false) return false;
            
            int bestTargetFaceIdx = FindBestAttachableFaceIndex(targetFaces, hitNormal, targetModule);
            if (bestTargetFaceIdx < 0)
                return false;
            targetFaceNormal = targetFaces[bestTargetFaceIdx].normal;
            targetFaceCenter = targetFaces[bestTargetFaceIdx].center;
            return true;
        }

        // 查找选中模块上最适合与目标面拼接的面
        private bool TryFindSourceAttachableFaceIndex(BaseModule selectedModule, Vector3 hitPoint)
        {
            var selectedAttach = selectedModule as IAttachable;
            if (selectedAttach == null)
                return false;
            var selectedFaces = selectedAttach.GetAttachableFaces();
            int bestSelectedFaceIdx = -1;
            float minDist = float.MaxValue;
            
            // 找到最近的可拼接面
            for (int i = 0; i < selectedFaces.Length; i++)
            {
                if (!selectedFaces[i].canAttach) continue;
                float dist = Vector3.Distance(selectedFaces[i].center, hitPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestSelectedFaceIdx = i;
                }
            }
            
            // 如果没找到可拼接面，返回失败
            if (bestSelectedFaceIdx < 0)
                return false;
            
            return true;
        }
        
        // 预览拼接的主函数
        private void TryPreviewAssemble(RaycastHit hit, BaseModule targetModule)
        {
            
            // 尝试找到目标可拼接面
            if (TryFindTargetAttachableFace(hit, targetModule, out Vector3 targetFaceNormal,
                    out Vector3 targetFaceCenter))
            {
                // 尝试找到源模块的最佳拼接面的下标
                if (TryFindSourceAttachableFaceIndex(_moduleSelector.SelectedModule, hit.point))
                {
                    Debug.Log($"展示预览...");
                    CreateOrUpdatePreview(_moduleSelector.SelectedModule, targetModule, targetFaceNormal, targetFaceCenter, hit.point);
                    
                    _showingPreview = true;
                    return;
                }
            }

            // 如果没有找到可拼接点，或者鼠标离开了模块，隐藏预览
            HidePreview();
        }
    }
}