// BuildController.cs
using UnityEngine;

namespace Script.Module
{

    public class BuildController : MonoBehaviour
    {

        //实例化
        public static BuildController Instance;
        [SerializeField, Header("插槽的层级")] private int socketLayer = 8;
        [SerializeField, Header("模块的层级")] private int moduleLayer = 7;    
        [Header("删除键"), SerializeField] private KeyCode removeButton = KeyCode.E;
        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
        }

        // 玩家是否选择了子插槽
        public bool IsActive => _selectedChildSocket != null;

        // 当前选中的子插槽
        public ModuleSocket CurrentChildSocket()
        {
            return _selectedChildSocket;
        }

        // 当前选中的子插槽
        private ModuleSocket _selectedChildSocket;

        [Header("游戏摄像头")] public Camera gameCamera;

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
                        TryRemoveModule(module);
                    }   
                }
                else
                {
                    // 尝试点击插槽
                    if (TryClickSocket(out ModuleSocket clickedSocket))
                    {
                        // 如果已经选择了子插槽，且点击的是空的父插槽，则尝试拼接
                        if (IsActive && !clickedSocket.IsAttached)
                        {
                            TryAssemble(clickedSocket);
                        }
                        // 如果点击的是空的插槽，且没有选择子插槽，则选择为子插槽
                        else if (!clickedSocket.IsAttached)
                        {
                            SelectChildSocket(clickedSocket);
                        }
                    }  
                }

            }

            // 右键取消选择
            if (Input.GetMouseButtonDown(1))
            {
                CancelSelection();
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

        void TryRemoveModule(BaseModule module)
        {
            if (module.parentModule == null) return;
            
            BaseModule parentModule = module.parentModule;
            ModuleSocket parentSocket = parentModule.FindSocketAttachedToModule(module);
            ModuleSocket childSocket = module.FindSocketAttachedToModule(parentModule);
            
            
            parentModule.RemoveChildModule(module);
            parentSocket.Detach();
            childSocket.Detach();
            
            
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
    }
}
