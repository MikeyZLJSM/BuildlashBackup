using System;
using Module.Enums;
using Module.Interfaces;
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

        public override ModuleFace[] GetAttachableFaces()
        {
            Vector3 localSize = _faceDetectCollider.size;
            Vector3 localExtents = localSize * 0.5f;
    
            _attachableFaces = new ModuleFace[6];
            
            _attachableFaces[0] = new ModuleFace(Vector3.up, new Vector3(0, localExtents.y, 0), true, this);
            _attachableFaces[1] = new ModuleFace(Vector3.down, new Vector3(0, -localExtents.y, 0), true, this);
            _attachableFaces[2] = new ModuleFace(Vector3.forward, new Vector3(0, 0, localExtents.z), true, this);
            _attachableFaces[3] = new ModuleFace(Vector3.back, new Vector3(0, 0, -localExtents.z), true, this);
            _attachableFaces[4] = new ModuleFace(Vector3.right, new Vector3(localExtents.x, 0, 0), true, this);
            _attachableFaces[5] = new ModuleFace(Vector3.left, new Vector3(-localExtents.x, 0, 0), true, this);
    
            return _attachableFaces;
        }
    }
}