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
        
    }
}

