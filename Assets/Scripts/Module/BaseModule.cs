using System.Collections;
using System.Collections.Generic;
using Scripts.Module.ModuleScript;
using UnityEngine;

namespace Scripts.Module
{
    public interface IAttachable
    {
        bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter, Vector3 hitPoint);
        // 返回所有可拼接面的法线和中心点
        (Vector3 normal, Vector3 center)[] GetAttachableFaces();
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    // 基础模块类，所有模块都应继承自此类
    // 该类可以包含一些通用的功能或属性供子类使用
    public abstract class BaseModule : MonoBehaviour, IAttachable
    {
        //TODO:枚举来代替string
        [Header("模块名称")] public string moduleName; // 模块的名称

        [Header("模块质量")] public float moduleMass = 1f; // 模块的质量，默认值为1

        // 父模块
        public BaseModule parentModule;

        // 模块的插槽列表
        [SerializeField] [Header("模块插槽列表")] public List<ModuleSocket> socketsList = new List<ModuleSocket>();

        // 模块的物理碰撞体
        protected Rigidbody _rb => GetComponent<Rigidbody>();

        //添加插槽的方法
        protected abstract void CreateSockets();

        protected virtual void Awake()
        {
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            CreateSockets();
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
            if (socketsList.Contains(childSocket)) return false;
            
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
            
            FixedJoint joint = childModule.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody    = _rb;               // 父模块刚体
            joint.breakForce       = Mathf.Infinity;    // 如需可破坏拼接，可设定阈值
            joint.breakTorque      = Mathf.Infinity;
            joint.enableCollision  = false;             // 若父子间不想互撞



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
                RemoveChildModule();
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
            // 恢复物理

            Destroy(childModule.GetComponent<FixedJoint>());
            childModule.SetPhysicsAttached(false); 
            childModule.GetComponent<Rigidbody>().AddForce(parentSideSocket.transform.forward * 10f, ForceMode.VelocityChange);
            childModule.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere*10f,ForceMode.VelocityChange);
        }

        public virtual void RemoveChildModule()
        {
            if (parentModule == null) return;

            // 断开父子关系
            transform.SetParent(null, true);

            // 置空父模块引用
            parentModule = null;

            // 移除 FixedJoint
            var joint = GetComponent<FixedJoint>();
            if (joint != null)
                Destroy(joint);

            // 恢复物理属性
            SetPhysicsAttached(false);

            // 弹开
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
                rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
            }
        }

        public void SetPhysicsAttached(bool attached)
        {
            if(moduleName == "BaseCube")
                return;
            
            if (attached)
            {
                // 禁用物理
                _rb.isKinematic = false;
                _rb.detectCollisions = true;
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

        // 默认实现：返回所有可拼接面的法线和中心点（立方体为例）
        public virtual (Vector3 normal, Vector3 center)[] GetAttachableFaces()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return new (Vector3, Vector3)[0];
            Vector3 center = transform.position;
            Vector3 half = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
            return new (Vector3, Vector3)[]
            {
                (transform.right,   center + transform.right * half.x),
                (-transform.right,  center - transform.right * half.x),
                (transform.up,      center + transform.up * half.y),
                (-transform.up,     center - transform.up * half.y),
                (transform.forward, center + transform.forward * half.z),
                (-transform.forward,center - transform.forward * half.z)
            };
        }

        // 默认实现：面对面拼接
        public virtual bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter, Vector3 hitPoint)
        {
            if(parentModule != null) return false;
            
            // 1. 找到自身所有面，选最近的面
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
            // 5. 建立父子关系和物理连接
            transform.SetParent(targetModule.transform, true);
            parentModule = targetModule;
            SetPhysicsAttached(true);
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = targetModule.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            joint.enableCollision = false;

            SetPhysicsAttached(true);
            
            return true;
        }
    }
}