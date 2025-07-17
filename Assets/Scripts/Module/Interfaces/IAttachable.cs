using UnityEngine;

namespace Module.Interfaces
{
    public interface IAttachable
    {
        bool AttachToFace(BaseModule targetModule, Vector3 targetNormal, Vector3 targetFaceCenter, Vector3 hitPoint, bool isPreview = false);

        // 返回所有可拼接面的法线和中心点
        (Vector3 normal, Vector3 center, bool canAttach)[] GetAttachableFaces();
    }
}