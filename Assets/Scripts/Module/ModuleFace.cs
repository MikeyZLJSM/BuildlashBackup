using Module;
using UnityEngine;

namespace Module
{ 
    public class ModuleFace
    {
        public Vector3 LocalNormal { get; set; }  // 局部空间法线
        public Vector3 LocalOffset { get; set; }  // 局部空间偏移
        public bool CanAttach { get; set; }
        public BaseModule Module { get; set; }
        public ModuleFace AttachedFace { get; set; }
    
        // 世界空间法线（动态计算）
        public Vector3 Normal => Module.transform.TransformDirection(LocalNormal);
    
        // 世界空间中心（动态计算）
        public Vector3 Center => Module.transform.TransformPoint(LocalOffset);
    
        public ModuleFace(Vector3 localNormal, Vector3 localOffset, bool canAttach, BaseModule module)
        {
            LocalNormal = localNormal;
            LocalOffset = localOffset;
            CanAttach = canAttach;
            Module = module;
        }
    }
}

