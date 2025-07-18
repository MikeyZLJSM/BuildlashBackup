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
    [RequireComponent(typeof(BoxCollider))]
    // 基础模块类，所有模块都应继承自此类
    // 该类可以包含一些通用的功能或属性供子类使用
    public abstract partial class BaseModule : MonoBehaviour, IAttachable
    {
        public ModuleType moduleType;
        public float moduleMass = 1f;
        public BaseModule parentModule;
        public List<BaseModule> childModules = new List<BaseModule>();
        protected (Vector3 normal, Vector3 center, bool canAttach)[] _attachableFaces;
        protected Rigidbody _rb;
        
        protected BoxCollider _faceDetectCollider;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            _faceDetectCollider = GetComponent<BoxCollider>();
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
            ModulesManager.Instance.OnModuleDetached(this);
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

        // 返回所有可拼接面的法线和中心点,以及面是否可拼接
        public virtual (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces()
        {
            return new (Vector3 normal, Vector3 center, bool canAttach)[]
                { };
        }
        protected virtual void SetAttachableFaces(int faceIndex)
        {
            if (_attachableFaces != null)
            {
                _attachableFaces[faceIndex].canAttach = false;
            }
        }

        // 默认实现：面对面拼接，将检测BoxCollider射线命中的面
        public virtual bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter,
            Vector3 hitPoint, bool isPreview = false)
        {
            if (parentModule) return false;

            //Debug.Log($"拼接到目标 模块：{targetModule.name} ，法向量{targetNormal} ，中心点{targetFaceCenter} ，射线检测点{hitPoint}");
            
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

            // 6. 通知ModulesManager模块已拼接
            if (!isPreview && ModulesManager.Instance != null)
            {
                ModulesManager.Instance.OnModuleAttached(targetModule, this);
            }

            // if (!isPreview)
            // {
            //     SetAttachableFaces(minIdx); 
            // }
            
            return true;
        }
    }
}