using System.Collections.Generic;
using System.Linq;
using Controllers;
using Module.Enums;
using Module.Interfaces;
using Module.ModuleScript;
using UnityEngine;

// 添加Controllers命名空间

namespace Module
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    // 基础模块类，所有模块都应继承自此类
    // 该类可以包含一些通用的功能或属性供子类使用
    public abstract partial class BaseModule : MonoBehaviour, IAttachable
    {
        public ModuleType moduleType;
        public float moduleMass = 1f;
        public BaseModule parentModule;
        public List<BaseModule> childModules = new List<BaseModule>();
        protected Rigidbody _rb;

        // 模块的插槽列表
        [SerializeField] [Header("模块插槽列表")] public List<ModuleSocket> socketsList = new();

        //添加插槽的方法
        protected abstract void CreateSockets();

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            CreateSockets();
        }

        public bool CanBeAttachedTarget()
        {
            if (moduleType == ModuleType.BaseCube)
            {
                return true;
            }

            if (!parentModule)
            {
                return false;
            }

            return parentModule.CanBeAttachedTarget();
        }

        // 添加子模块的方法
        public virtual bool AttachChildModule(BaseModule childModule,
            ModuleSocket parentSocket,
            ModuleSocket childSocket)
        {
            if (parentSocket.IsAttached) return false;
            if (childSocket.IsAttached) return false;
            if (childModule == null) return false;
            if (childModule.GetType() == typeof(BaseCube)) return false;
            if (childModule.parentModule != null) return false;
            if (!socketsList.Contains(parentSocket)) return false; // 检查父插槽是否属于当前模块
            if (!childModule.socketsList.Contains(childSocket)) return false; // 检查子插槽是否属于子模块

            // 旋转对齐：让子插槽的forward方向与父插槽的forward方向完全相反
            var parentForward = parentSocket.transform.forward;

            // 计算从子插槽当前朝向到目标朝向（与父插槽相反）的旋转
            var targetChildSocketRotation = Quaternion.LookRotation(-parentForward, parentSocket.transform.up);
            var currentChildSocketRotation = childSocket.transform.rotation;
            var rotationOffset = targetChildSocketRotation * Quaternion.Inverse(currentChildSocketRotation);

            // 应用旋转到整个子模块
            childModule.transform.rotation = rotationOffset * childModule.transform.rotation;

            // 位置对齐：让两个插槽的位置重合
            var offset = childSocket.transform.position - childModule.transform.position;
            childModule.transform.position = parentSocket.transform.position - offset;

            //分级
            childModule.transform.SetParent(transform, true);


            parentSocket.Attach(childModule); // 父端插槽记录子模块
            childSocket.Attach(this); // 子端插槽记录父模块
            childModule.parentModule = this;

            // 添加到子模块列表
            AddChildModuleToList(childModule);

            //关闭物理
            childModule.SetPhysicsAttached(true);

            var joint = childModule.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = _rb; // 父模块刚体
            joint.breakForce = Mathf.Infinity; // 如需可破坏拼接，可设定阈值
            joint.breakTorque = Mathf.Infinity;
            joint.enableCollision = false; // 若父子间不想互撞

            // 通知ScriptManager模块已拼接
            if (ScriptManager.Instance != null) ScriptManager.Instance.OnModuleAttached(this, childModule);

            return true;
        }

        // 虚方法：无插槽拼接，子类可重写
        public virtual bool AttachChildModuleNoSocket(BaseModule childModule)
        {
            // 默认不支持无插槽拼接
            return false;
        }

        public virtual void RemoveChildModule(BaseModule childModule, bool socketJoint = true)
        {
            if (socketJoint)
            {
                RemoveChildModule(FindSocketAttachedToModule(childModule));
            }
            else
            {
                RemoveModule();
            }
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

            // 从子模块列表中移除
            RemoveChildModuleFromList(childModule);

            // 恢复物理

            Destroy(childModule.GetComponent<FixedJoint>());
            childModule.SetPhysicsAttached(false);
            childModule.GetComponent<Rigidbody>()
                .AddForce(parentSideSocket.transform.forward * 10f, ForceMode.VelocityChange);
            childModule.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
        }

        public virtual void RemoveModule()
        {
            if (!parentModule) return;

            parentModule.RemoveChildModuleFromList(this);
            transform.SetParent(null, true);
            parentModule = null;

            SetPhysicsAttached(false);
            if (!_rb) return;
            _rb.AddForce(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
            _rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.VelocityChange);

            foreach (var child in childModules.ToList())
            {
                if (!child) continue;
                child.RemoveModule();
            }

            // 通知ScriptManager模块已拆除
            ScriptManager.Instance.OnModuleDetached(this);
        }

        public void AddChildModuleToList(BaseModule childModule)
        {
            if (childModule != null && !childModules.Contains(childModule))
            {
                childModules.Add(childModule);
            }
        }

        public void RemoveChildModuleFromList(BaseModule childModule)
        {
            if (childModules.Contains(childModule))
            {
                childModules.Remove(childModule);
            }
        }

        public void SetPhysicsAttached(bool attached)
        {
            if (moduleType == ModuleType.BaseCube)
                return;

            if (attached)
            {
                _rb.isKinematic = true;
            }
            else
            {
                _rb.isKinematic = false;
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

        // 返回所有可拼接面的法线和中心点,以及面是否可拼接
        public virtual (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces()
        {
            return new (Vector3 normal, Vector3 center, bool canAttach)[]
                { };
        }

        // 默认实现：面对面拼接（当前是立方体的默认实现）
        public virtual bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter,
            Vector3 hitPoint)
        {
            if (parentModule != null) return false;

            // 归一化旋转到最近的90度，防止受重力影响之后无法对齐拼接面
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(euler);

            // 1. 找到自身所有可拼接面，选最近的面
            var faces = GetAttachableFaces();
            int minIdx = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < faces.Length; i++)
            {
                float dist = Vector3.Distance(faces[i].center, hitPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }
            }

            Vector3 selfNormal = faces[minIdx].normal;
            Vector3 selfCenter = faces[minIdx].center;
            // 2. 旋转自身，使该面法线与目标法线相反
            Quaternion rotation = Quaternion.FromToRotation(selfNormal, -targetNormal);
            transform.rotation = rotation * transform.rotation;
            // 3. 旋转后重新计算该面中心
            selfNormal = rotation * selfNormal;
            selfCenter = transform.position + selfNormal * (selfCenter - transform.position).magnitude;
            // 4. 平移自身，使该面中心与目标面中心重合
            Vector3 offset = targetFaceCenter - selfCenter;
            transform.position += offset;
            // 5. 建立父子关系
            transform.SetParent(targetModule.transform, true);
            parentModule = targetModule;
            targetModule.AddChildModuleToList(this);
            SetPhysicsAttached(true);

            return true;
        }
    }
}