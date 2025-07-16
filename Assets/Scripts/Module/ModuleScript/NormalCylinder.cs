using System;
using System.Collections;
using System.Collections.Generic;
using Module.Enums;
using UnityEngine;


namespace Module.ModuleScript
{
    public class NormalCylinder : BaseModule
    {
        protected override void CreateSockets()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            moduleType = ModuleType.NormalCylinder;
        }

        public override (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces()
        {
            BoxCollider cld = GetComponent<BoxCollider>();
            if (!cld) return Array.Empty<(Vector3, Vector3, bool)>();

            Vector3 center = transform.position;
            Vector3 up = transform.up.normalized;

            Vector3 half = Vector3.Scale(cld.size * 0.5f, transform.lossyScale);

            return new[]
            {
                (up, center + transform.up * half.y, true),
                (-up, center - transform.up * half.y, true),
            };
        }
        
        public override bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter,
            Vector3 hitPoint)
        {
            if (parentModule != null) return false;

            // 归一化旋转到最近的90度，防止受重力影响之后无法对齐拼接面
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90f) * 90f;
            euler.y = Mathf.Round(euler.y / 90f) * 90f;
            euler.z = Mathf.Round(euler.z / 90f) * 90f;
            transform.rotation = Quaternion.Euler(euler);

            // 获取可拼接面
            var faces = GetAttachableFaces();
            
            // 只考虑上下两个圆形底面
            int minIdx = -1;
            float minDist = float.MaxValue;
            
            // 只检查上下两个面
            for (int i = 0; i < faces.Length; i++)
            {
                // 确保只检查圆形底面
                if (faces[i].canAttach)
                {
                    float dist = Vector3.Distance(faces[i].center, hitPoint);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIdx = i;
                    }
                }
            }
            
            // 如果没找到合适的面，返回失败
            if (minIdx < 0)
                return false;

            Vector3 selfNormal = faces[minIdx].normal;
            Vector3 selfCenter = faces[minIdx].center;
            // 旋转自身，使该面法线与目标法线相反
            Quaternion rotation = Quaternion.FromToRotation(selfNormal, -targetNormal);
            transform.rotation = rotation * transform.rotation;
            // 旋转后重新计算该面中心
            selfNormal = rotation * selfNormal;
            selfCenter = transform.position + selfNormal * (selfCenter - transform.position).magnitude;
            // 平移自身，使该面中心与目标面中心重合
            Vector3 offset = targetFaceCenter - selfCenter;
            transform.position += offset;
            // 建立父子关系
            transform.SetParent(targetModule.transform, true);
            parentModule = targetModule;
            targetModule.AddChildModuleToList(this);
            SetPhysicsAttached(true);

            return true;
        }
        
    }
}

