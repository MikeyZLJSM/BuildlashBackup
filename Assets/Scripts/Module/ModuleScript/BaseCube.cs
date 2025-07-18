using System;
using Module.Enums;
using UnityEngine;

namespace Module.ModuleScript
{
    // 基础立方体模块
    [AddComponentMenu("Modules/CubeModule")]
    public sealed class BaseCube : NormalCube
    {
        protected override void Awake()
        {
            base.Awake();
            
            _rb.isKinematic = true;
            _rb.detectCollisions = true;
            moduleType = ModuleType.BaseCube;
        }
    }
}
