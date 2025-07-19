using System;
using System.Collections;
using System.Collections.Generic;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;


namespace Module.ModuleScript
{
    public class NormalCylinder : BaseModule
    {
        protected override void Awake()
        {
            base.Awake();
            moduleType = ModuleType.NormalCylinder;
        }

        public override ModuleFace[] GetAttachableFaces()
        {
            Vector3 localSize = _faceDetectCollider.size;
            Vector3 localExtents = localSize * 0.5f;
    
            _attachableFaces = new ModuleFace[2];
            
            _attachableFaces[0] = new ModuleFace(Vector3.up, new Vector3(0, localExtents.y, 0), true, this);
            _attachableFaces[1] = new ModuleFace(Vector3.down, new Vector3(0, -localExtents.y, 0), true, this);
            
            return _attachableFaces;
        }
        
    }
}

