using System;
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
        public List<BaseModule> _childModules = new List<BaseModule>();
        
        public ModuleFace[] _attachableFaces;

        protected Rigidbody _rb;
        
        protected BoxCollider _faceDetectCollider;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = moduleMass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            _faceDetectCollider = GetComponent<BoxCollider>();
            _attachableFaces = GetAttachableFaces();
        }

        public bool CanBeAttachedTarget_r()
        {
            if (moduleType == ModuleType.BaseCube)
            {
                return true;
            }

            if (!parentModule)
            {
                return false;
            }

            return parentModule.CanBeAttachedTarget_r();
        }

        public virtual void RemoveModule()
        {
            if (!parentModule) return;

            SetAllFacesDetach();
            
            parentModule.RemoveChildModuleFromList(this);
            transform.SetParent(null, true);
            parentModule = null;

            SetPhysicsAttached(false);
            if (!_rb) return;
            _rb.AddForce(UnityEngine.Random.onUnitSphere * 10f, ForceMode.VelocityChange);
            _rb.AddTorque(UnityEngine.Random.onUnitSphere * 10f, ForceMode.VelocityChange);

            foreach (var child in _childModules.ToList())
            {
                if (!child) continue;
                child.RemoveModule();
            }

            // 通知ScriptManager模块已拆除
            ModulesManager.Instance.OnModuleDetached(this);
        }

        public void AddChildModuleToList(BaseModule childModule)
        {
            if (childModule != null && !_childModules.Contains(childModule))
            {
                _childModules.Add(childModule);
            }
        }

        public void RemoveChildModuleFromList(BaseModule childModule)
        {
            if (_childModules.Contains(childModule))
            {
                _childModules.Remove(childModule);
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
        // 改进的 BaseModule.GetAttachableFaces 方法
        public virtual ModuleFace[] GetAttachableFaces()
        {
            // 直接使用 BoxCollider 的 size，这是局部空间的尺寸
            Vector3 localSize = _faceDetectCollider.size;
            Vector3 localExtents = localSize * 0.5f;  // 半尺寸
    
            // 创建6个面的ModuleFace对象
            ModuleFace[] faces = new ModuleFace[6];
    
            // 上面 +Y
            faces[0] = new ModuleFace(
                Vector3.up,
                new Vector3(0, localExtents.y, 0),
                false,
                this
            );
    
            // 下面 -Y
            faces[1] = new ModuleFace(
                Vector3.down,
                new Vector3(0, -localExtents.y, 0),
                false,
                this
            );
    
            // 前面 +Z
            faces[2] = new ModuleFace(
                Vector3.forward,
                new Vector3(0, 0, localExtents.z),
                false,
                this
            );
    
            // 后面 -Z
            faces[3] = new ModuleFace(
                Vector3.back,
                new Vector3(0, 0, -localExtents.z),
                false,
                this
            );
    
            // 右面 +X
            faces[4] = new ModuleFace(
                Vector3.right,
                new Vector3(localExtents.x, 0, 0),
                false,
                this
            );
    
            // 左面 -X
            faces[5] = new ModuleFace(
                Vector3.left,
                new Vector3(-localExtents.x, 0, 0),
                false,
                this
            );
    
            return faces;
        }

        // 默认实现：面对面拼接，将检测BoxCollider射线命中的面
        // 修改 BaseModule.Attach.cs 中的 AttachToFace 方法
        public virtual bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter,
            Vector3 hitPoint, bool isPreview = false)
        {
            if (parentModule) return false;

            // 归一化旋转到最近的90度，防止受重力影响之后无法对齐拼接面
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(euler);

            // 1. 重新获取当前的可拼接面（确保使用最新的位置信信息）

            // 1. 找到自身所有可拼接面，选最近的面
            int minIdx = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < _attachableFaces.Length; i++)
            {
                float dist = Vector3.Distance(_attachableFaces[i].Center, hitPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }
            }

            ModuleFace sourceFace = _attachableFaces[minIdx];
            
            ModuleFace[] targetFaces = targetModule._attachableFaces;
            ModuleFace targetFace = null;
            int targetFaceIdx = -1;
            for (int i = 0; i < targetFaces.Length; i++)
            {
                if (targetNormal == targetFaces[i].Normal)
                {
                    targetFace = targetFaces[i];
                    targetFaceIdx = i;  // 这里就是你要的数组下标
                    break;
                }
            }

            // 2. 直接使用局部偏移，而不是转换世界坐标
            Vector3 localFaceOffset = sourceFace.LocalOffset;

            // 3. 旋转自身，使该面法线与目标法线相反
            Quaternion rotation = Quaternion.FromToRotation(sourceFace.Normal, -targetNormal);
            transform.rotation = rotation * transform.rotation;

            // 4. 使用局部偏移重新计算旋转后的面中心
            Vector3 rotatedFaceCenter = transform.TransformPoint(localFaceOffset);

            // 5. 平移自身，使该面中心与目标面中心重合
            Vector3 offset = targetFaceCenter - rotatedFaceCenter;
            transform.position += offset;

            // 6. 建立父子关系
            transform.SetParent(targetModule.transform, true);
            parentModule = targetModule;
            targetModule.AddChildModuleToList(this);
            SetPhysicsAttached(true);

            // 7. 通知ModulesManager模块已拼接
            if (ModulesManager.Instance != null)
            {
                ModulesManager.Instance.OnModuleAttached(targetModule, this);
            }

            if (!isPreview && targetFace != null)
            {
                SetFaceAttachByIndex(minIdx, targetFaceIdx, targetModule);
            }

            return true;
        }

        private void SetFaceAttachByIndex(int sourceFaceIdx, int targetFaceIdx , BaseModule targetModule)
        {
            if(!_attachableFaces[sourceFaceIdx].CanAttach) return;
            _attachableFaces[sourceFaceIdx].AttachedFace = targetModule._attachableFaces[targetFaceIdx];
            _attachableFaces[sourceFaceIdx].CanAttach = false;
            
            targetModule.SetFaceAttachByIndex(targetFaceIdx, sourceFaceIdx, this);
        }

        private void SetAllFacesDetach()
        {
            if(moduleType == ModuleType.BaseCube) return;
            
            if (_attachableFaces != null)
            {
                foreach (var face in _attachableFaces)
                {
                    if (face.AttachedFace == null) continue;
                    face.AttachedFace = null;
                    face.CanAttach = true;
                }
            }
            
            foreach (var childModule in _childModules.ToList())
            {
                childModule.SetAllFacesDetach();
            }
            
            parentModule.SetOneFaceDetach(this);
        }
        
        //用于父模块断开与当前拆除模块的面
        private void SetOneFaceDetach(BaseModule childModule)
        {
            if (_attachableFaces == null) return;

            foreach (var face in _attachableFaces)
            {
                if (face.AttachedFace?.Module == childModule)
                {
                    face.AttachedFace = null;
                    face.CanAttach = true;
                    break;
                }
            }
        }
        
    }
}