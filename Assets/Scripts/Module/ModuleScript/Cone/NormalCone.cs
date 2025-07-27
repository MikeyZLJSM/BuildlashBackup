using Module.Enums;
using UnityEngine;

namespace Module.ModuleScript.Cone
{
    public class NormalCone : BaseModule
    {
        protected override void Awake()
        {
            base.Awake();
            moduleType = ModuleType.NormalCone;
        }

        public override ModuleFace[] GetAttachableFaces()
        {
            Vector3 localSize = _faceDetectCollider.size;
            Vector3 localExtents = localSize * 0.5f;
    
            _attachableFaces = new ModuleFace[1];
            
            _attachableFaces[0] = new ModuleFace(Vector3.down, new Vector3(0, -localExtents.y, 0), true, this);
    
            return _attachableFaces;
        }
        
    }
}