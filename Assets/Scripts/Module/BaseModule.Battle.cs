using System;
using Module.Battle;
using Module.Enums;

namespace Module
{
    public abstract partial class BaseModule
    {
        /// <summary>生命值</summary>
        [NonSerialized] public int _health = 100;
        // 攻击参数
        public AttackParameters _attackParameters;
        public float _attackCD;
        public bool _canAttack;
    }
}