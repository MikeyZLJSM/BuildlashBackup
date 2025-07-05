using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Module
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    // 基础模块类，所有模块都应继承自此类
    // 该类可以包含一些通用的功能或属性供子类使用
    public abstract class BaseModule : MonoBehaviour
    {
        [Header("模块名称")] public string moduleName; // 模块的名称

        [Header("模块质量")] public float moduleMass = 1f; // 模块的质量，默认值为1


        // 父模块
        public BaseModule parentModule;

        // 模块的插槽列表
        [SerializeField] [Header("模块插槽列表")] public List<ModuleSocket> socketsList = new List<ModuleSocket>();

        // 模块的物理碰撞体
        protected Rigidbody _rb;

        //添加插槽的方法
        protected abstract void CreateSockets();

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            CreateSockets();
        }

        // 添加子模块的方法
        public virtual bool AttachChildModule(BaseModule childModule,
            ModuleSocket parentSocket,
            ModuleSocket childSocket)
        {
            // 
            if (parentSocket.IsAttached) return false;
            if (childSocket.IsAttached) return false;
            if (childModule == null) return false;

            // 旋转对齐：让子插槽的forward方向与父插槽的forward方向完全相反
            Vector3 parentForward = parentSocket.transform.forward;
            
            // 计算从子插槽当前朝向到目标朝向（与父插槽相反）的旋转
            Quaternion targetChildSocketRotation = Quaternion.LookRotation(-parentForward, parentSocket.transform.up);
            Quaternion currentChildSocketRotation = childSocket.transform.rotation;
            Quaternion rotationOffset = targetChildSocketRotation * Quaternion.Inverse(currentChildSocketRotation);
            
            // 应用旋转到整个子模块
            childModule.transform.rotation = rotationOffset * childModule.transform.rotation;

            // 位置对齐：让两个插槽的位置重合
            Vector3 offset = childSocket.transform.position - childModule.transform.position;
            childModule.transform.position = parentSocket.transform.position - offset;

            //分级
            childModule.transform.SetParent(this.transform, true);

            
            parentSocket.Attach(childModule); // 父端插槽记录子模块
            childSocket.Attach(this); // 子端插槽记录父模块
            childModule.parentModule = this;

            //关闭物理
            childModule.SetPhysicsAttached(true);

            return true;
        }

        // 拆除子模块的方法
        public virtual void RemoveChildModule(ModuleSocket parentSideSocket)
        {
            
            if (parentSideSocket == null || !parentSideSocket.IsAttached) return;

            BaseModule childModule = parentSideSocket.AttachedModule;

            //断层级
            childModule.transform.SetParent(null, true); // 脱出父层级，保留世界位置

            // 置空插槽
            parentSideSocket.Attach(null); // 父端插槽置空
            ModuleSocket childSideSocket = childModule.FindSocketAttachedToModule(this);
            if (childSideSocket != null) childSideSocket.Attach(null);

            childModule.parentModule = null;

            // 恢复物理
            childModule.SetPhysicsAttached(false); // 重新启用刚体
        }

        public void SetPhysicsAttached(bool attached)
        {
            if (attached)
            {
                // 禁用物理
                _rb.isKinematic = true;
                _rb.detectCollisions = false;
            }
            else
            {
                // 恢复物理
                _rb.isKinematic = false;
                _rb.detectCollisions = true;
            }
        }

        // 查找附加到指定子模块的插槽
        public ModuleSocket FindSocketAttachedToModule(BaseModule targetChildModule)
        {
            foreach (ModuleSocket moduleSocket in socketsList)
            {
                if (moduleSocket.AttachedModule == targetChildModule)
                {
                    return moduleSocket; // 返回找到的插槽
                }
            }

            return null; // 如果没有找到，返回null
        }
    }
}