using System;
using System.Collections;
using System.Collections.Generic;
using Module.Enums;
using UnityEngine;


namespace Module.ModuleScript
{
    public class NormalCone : BaseModule
    {
        protected override void CreateSockets()
        {
        }
        
        private BoxCollider _collider;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<BoxCollider>();
            moduleType = ModuleType.NormalCylinder;
        }

        public override (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces()
        {
            if (!_collider)
            {
                Debug.LogError("碰撞体为空");
                return Array.Empty<(Vector3, Vector3, bool)>();
            }
            Vector3 center = transform.position;
            Vector3 up = transform.up.normalized;
            Vector3 half = Vector3.Scale(_collider.size * 0.5f, transform.lossyScale);

            return new[]
            {
                (-up, center - transform.up * half.y, true),
            };
        }
        
    }
}