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

        public List<BaseModule> childModules = new();

        // 模块的插槽列表
        [SerializeField] [Header("模块插槽列表")] public List<ModuleSocket> socketsList = new();

        protected Rigidbody _rb;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            CreateSockets();
        }

        // 默认实现：返回所有可拼接面的法线和中心点（当前是立方体的默认实现）
        public virtual (Vector3 normal, Vector3 center)[] GetAttachableFaces()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return new (Vector3, Vector3)[0];
            var center = transform.position;
            var half = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
            return new[]
            {
                (transform.right, center + transform.right * half.x),
                (-transform.right, center - transform.right * half.x),
                (transform.up, center + transform.up * half.y),
                (-transform.up, center - transform.up * half.y),
                (transform.forward, center + transform.forward * half.z),
                (-transform.forward, center - transform.forward * half.z)
            };
        }

        // 默认实现：面对面拼接（当前是立方体的默认实现）
        public virtual bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter,
            Vector3 hitPoint)
        {
            if (parentModule != null) return false;

            // 归一化旋转到最近的90度，防止受重力影响之后无法对齐拼接面
            var euler = transform.rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(euler);

            // 1. 找到自身所有面，选最近的面
            var faces = GetAttachableFaces();
            var minIdx = 0;
            var minDist = float.MaxValue;
            for (var i = 0; i < faces.Length; i++)
            {
                var dist = Vector3.Distance(faces[i].center, hitPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }
            }

            var selfNormal = faces[minIdx].normal;
            var selfCenter = faces[minIdx].center;
            // 2. 旋转自身，使该面法线与目标法线相反
            var rotation = Quaternion.FromToRotation(selfNormal, -targetNormal);
            transform.rotation = rotation * transform.rotation;
            // 3. 旋转后重新计算该面中心
            selfNormal = rotation * selfNormal;
            selfCenter = transform.position + selfNormal * (selfCenter - transform.position).magnitude;
            // 4. 平移自身，使该面中心与目标面中心重合
            var offset = targetFaceCenter - selfCenter;
            transform.position += offset;
            // 5. 建立父子关系和物理连接
            transform.SetParent(targetModule.transform, true);
            parentModule = targetModule;

            // 6. 更新子模块关系
            targetModule.AddChildModule(this);

            // 通知ScriptManager模块已拼接
            if (ScriptManager.Instance != null) ScriptManager.Instance.OnModuleAttached(targetModule, this);

            //TODO: 建造阶段可以先不把刚体连接起来，等到游戏开始才连接刚体
            SetPhysicsAttached(true);
            // FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            // joint.connectedBody = targetModule.GetComponent<Rigidbody>();
            // joint.breakForce = Mathf.Infinity;
            // joint.breakTorque = Mathf.Infinity;
            // joint.enableCollision = false;

            SetPhysicsAttached(true);

            return true;
        }

        //添加插槽的方法
        protected abstract void CreateSockets();
    
        //<summary>
        /// 添加子模块的方法（插槽实现）
        // </summary>
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
            AddChildModule(childModule);

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


        // <summary>
        /// （插槽方法，未使用）
        // </summary>
        public virtual void RemoveChildModule(BaseModule childModule, bool socketJoint = true)
        {
            if (socketJoint)
                RemoveChildModule(FindSocketAttachedToModule(childModule));
            else
                RemoveModule();
        }

        // <summary>
        // 拆除子模块的方法
        // </summary>
        public virtual void RemoveChildModule(ModuleSocket parentSideSocket)
        {
            if (parentSideSocket == null || !parentSideSocket.IsAttached) return;

            var childModule = parentSideSocket.AttachedModule;

            //断层级
            childModule.transform.SetParent(null, true); // 脱出父层级，保留世界位置

            // 置空插槽
            parentSideSocket.Attach(null); // 父端插槽置空
            var childSideSocket = childModule.FindSocketAttachedToModule(this);
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

        
        // <summary>
        /// 移除当前模块（包括所有子模块）
        // </summary>
        public virtual void RemoveModule()
        {
            if (!parentModule) return;

            parentModule.RemoveChildModuleFromList(this);
            transform.SetParent(null, true);
            parentModule = null;

            // var joint = GetComponent<FixedJoint>();
            // if (joint != null)
            //     Destroy(joint);

            SetPhysicsAttached(false);
            if (!_rb) return;
            _rb.AddForce(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
            _rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
            
            // 通知ScriptManager模块已拆除
            ScriptManager.Instance.OnModuleDetached(this);

            foreach (var child in childModules.ToList()) child.RemoveModule();
        }

        // 添加子模块到列表
        public void AddChildModule(BaseModule childModule)
        {
            if (childModule != null && !childModules.Contains(childModule)) childModules.Add(childModule);
        }

        // 从列表中移除子模块
        public void RemoveChildModuleFromList(BaseModule childModule)
        {
            if (childModules.Contains(childModule)) childModules.Remove(childModule);
        }

        // 获取当前模块的所有子模块
        public List<BaseModule> GetAllChildModules()
        {
            return new List<BaseModule>(childModules);
        }

        // 获取所有子模块（递归，包括子模块的子模块）
        public List<BaseModule> GetAllChildModulesRecursive()
        {
            var allChildren = new List<BaseModule>();
            foreach (var child in childModules)
            {
                allChildren.Add(child);
                allChildren.AddRange(child.GetAllChildModulesRecursive());
            }

            return allChildren;
        }

        public void SetPhysicsAttached(bool attached)
        {
            //TODO:递归地把每一个子模块的物理属性重置一遍
            if (moduleType == ModuleType.BaseCube)
                return;

            if (attached)
                // 禁用物理
                _rb.isKinematic = true;
            else
                // 恢复物理
                _rb.isKinematic = false;
        }

        // 查找附加到指定子模块的插槽
        public ModuleSocket FindSocketAttachedToModule(BaseModule targetChildModule)
        {
            foreach (var moduleSocket in socketsList)
                if (moduleSocket.AttachedModule == targetChildModule)
                    return moduleSocket; // 返回找到的插槽

            return null; // 如果没有找到，返回null
        }
    }
}