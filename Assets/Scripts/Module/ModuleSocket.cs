﻿using Controllers;
using UnityEngine;

namespace Module
{
    // 管理插槽和模块的吸附
    public class ModuleSocket : MonoBehaviour
    {
        public BaseModule parentModule; // 插槽所属的模块
        public BaseModule AttachedModule { get; private set; } // 附加的模块
        public bool IsAttached => AttachedModule != null; // 是否有附加模块

        public void Attach(BaseModule attachedModule)
        {
            AttachedModule = attachedModule;
            GetComponent<SocketSelector>().SetNormal();
        }

        public void Detach()
        {
            Attach(null);
        }
    }
}