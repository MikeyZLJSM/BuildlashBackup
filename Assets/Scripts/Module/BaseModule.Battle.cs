using System;
using Module.Battle;
using Module.Enums;

namespace Module
{
    public abstract partial class BaseModule
    {
        /// <summary>生命值</summary>
        [NonSerialized] public int _health = 100;
    }
}