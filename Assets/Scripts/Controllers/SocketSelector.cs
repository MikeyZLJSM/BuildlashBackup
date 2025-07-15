// SocketSelector.cs

using Module;
using UnityEngine;

namespace Controllers
{
    //负责切换插槽颜色。
    [RequireComponent(typeof(ModuleSocket))]
    [RequireComponent(typeof(Renderer))]
    public class SocketSelector : MonoBehaviour
    {
        [Header("插槽状态颜色")] [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color pickedColor = Color.cyan;
        [SerializeField] private Color attchedColor = Color.green;
        [SerializeField] public ModuleSocket thisSocket;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (thisSocket == null) Debug.LogError("SocketSelector找不到引用");
        }

        //处理鼠标事件
        private void OnMouseEnter()
        {
            // 如果当前插槽已被选中，保持选中颜色
            if (BuildController.Instance.CurrentChildSocket() == GetSocket())
            {
                SetPicked();
                return;
            }

            // 如果插槽未附加，显示悬停效果
            if (!GetSocket().IsAttached) SetHover();
        }

        private void OnMouseExit()
        {
            // 若此插槽当前是 BuildController 的“正在高亮”则保持 picked 色
            if (BuildController.Instance.CurrentChildSocket() == GetSocket())
                SetPicked();
            else
                SetNormal();
        }

        //设置不同颜色
        public void SetNormal()
        {
            _renderer.material.color = normalColor;
            if (thisSocket.IsAttached) _renderer.material.color = attchedColor;
        }

        public void SetHover()
        {
            _renderer.material.color = hoverColor;
        }

        public void SetPicked()
        {
            _renderer.material.color = pickedColor;
        }

        /* ---------- 私有 ---------- */
        private ModuleSocket GetSocket()
        {
            return GetComponent<ModuleSocket>();
        }
    }
}