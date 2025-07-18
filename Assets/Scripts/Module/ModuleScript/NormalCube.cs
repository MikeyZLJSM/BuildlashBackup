using System;
using Module.Enums;
using UnityEngine;

namespace Module.ModuleScript
{
    [AddComponentMenu("Modules/NormalCube")]
    public class NormalCube : BaseModule
    {
        protected override void Awake()
        {
            base.Awake();
            moduleType = ModuleType.NormalCube;
        }

        public override (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces()
        {
            if (!_faceDetectCollider)
            {
                Debug.LogError("碰撞体为空");
                return Array.Empty<(Vector3, Vector3, bool)>();
            }
            Vector3 center = transform.position;
            Vector3 half = Vector3.Scale(_faceDetectCollider.size * 0.5f, transform.lossyScale);

            return new[]
            {
                (transform.right, center + transform.right * half.x, true),
                (-transform.right, center - transform.right * half.x, true),
                (transform.up, center + transform.up * half.y, true),
                (-transform.up, center - transform.up * half.y, true),
                (transform.forward, center + transform.forward * half.z, true),
                (-transform.forward, center - transform.forward * half.z, true)
            };
        }
    }
}
