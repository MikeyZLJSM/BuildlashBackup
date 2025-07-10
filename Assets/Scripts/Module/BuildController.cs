// BuildController.cs
using UnityEngine;

namespace Scripts.Module
{

    public class BuildController : MonoBehaviour
    {
        //实例化
        public static BuildController Instance;

        [Header("游戏摄像头")] public Camera gameCamera;
        [SerializeField, Header("插槽的层级")] private int socketLayer = 8;
        [SerializeField, Header("模块的层级")] private int moduleLayer = 7;    
        [Header("删除键"), SerializeField] private KeyCode removeButton = KeyCode.E;

        public bool IsActive => _selectedChildSocket != null;
        private ModuleSocket _selectedChildSocket;
        public bool IsModuleSelected => _selectedModule != null;
        private BaseModule _selectedModule;

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
        }
        

        public ModuleSocket CurrentChildSocket()
        {
            return _selectedChildSocket;
        }
        

        // 选择子插槽
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

            print("选中子插槽: " + _selectedChildSocket.name);
        }

        // 取消选择
        public void CancelSelection()
        {
            if (_selectedChildSocket != null)
            {
                var socketSelector = _selectedChildSocket.GetComponent<SocketSelector>();
                if (socketSelector != null)
                    socketSelector.SetNormal();
            }

            _selectedChildSocket = null;
            print("取消选择");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.GetKey(removeButton))
                {
                    if (TryClickModule(out BaseModule module))
                    {
                        TryRemoveModule(module, false);
                    }   
                }
                else
                {
                    if (TryClickModule(out BaseModule clickedModule))
                    {
                        if (!IsModuleSelected)
                        {
                            if(clickedModule.moduleName == "BaseCube")
                                return;
                            SelectModule(clickedModule);
                        }
                        // 如果已经选中了一个模块，并且点击的是另一个模块，则尝试拼接
                        else if (_selectedModule != clickedModule)
                        {
                            TryAssembleModule(_selectedModule, clickedModule);
                        }
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CancelModuleSelection();
            }
        }
    
        void TryAssemble(ModuleSocket parentSocket)
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
                print("拼接成功!");
                CancelSelection();
                //更新父插槽颜色
                parentSocket.GetComponent<SocketSelector>().SetNormal();
            }
            else
            {
                print("拼接失败!");
            }
        }

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
        
        private void TryRemoveModule(BaseModule parentModule, bool socketJoint)
        {
            parentModule.RemoveChildModule(parentModule, socketJoint);
        }
        
        // 尝试点击插槽
        bool TryClickSocket(out ModuleSocket socket)
        {
            print("尝试点击插槽...");
            socket = null;

            // 射线检测 Socket 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit, 100f,
                    1 << socketLayer,QueryTriggerInteraction.Collide)){
                print("没有点击到插槽!");
                return false;}

            // 尝试取出组件
            socket = hit.collider.GetComponent<ModuleSocket>();
            return socket != null;
        }

        bool TryClickModule(out BaseModule module)
        {
            print("尝试点击模块");
            module = null;
            
            // 射线检测 Module 层
            if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit, 100f,
                    1 << moduleLayer,QueryTriggerInteraction.Collide)){
                print("没有点击到模块!");
                return false;}
            
            module = hit.collider.GetComponent<BaseModule>();
            return module != null;
        }

        public void SelectModule(BaseModule module)
        {
            //TODO：高亮显示选中模块
            _selectedModule = module;
            Debug.Log($"选中模块: {_selectedModule.moduleName}");
        }

        public void CancelModuleSelection()
        {
            _selectedModule = null;
            Debug.Log("取消模块选择");
        }

        // 模块对模块拼接（接支持所有实现 IAttachable 的模块）
        private void TryAssembleModule(BaseModule fromModule, BaseModule toModule)
        {
            var fromAttach = fromModule as IAttachable;
            var toAttach = toModule as IAttachable;
            if (fromAttach != null && toAttach != null)
            {
                Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << moduleLayer, QueryTriggerInteraction.Collide))
                {
                    Vector3 targetNormal = hit.normal; // 父模块被点击面的法线
                    // 计算父模块被点击面的中心点
                    var faces = toAttach.GetAttachableFaces();
                    int bestIdx = 0;
                    float bestDot = -1f;
                    for (int i = 0; i < faces.Length; i++)
                    {
                        float dot = Vector3.Dot(faces[i].normal, targetNormal);
                        if (dot > bestDot)
                        {
                            bestDot = dot;
                            bestIdx = i;
                        }
                    }
                    Vector3 targetFaceCenter = faces[bestIdx].center;
                    bool ok = fromAttach.AttachToFace(toModule, targetNormal, targetFaceCenter, hit.point);
                    if (ok)
                    {
                        CancelModuleSelection();
                    }
                }
            }
        }
    }
}
