using System;
using Module.Enums;

namespace Module
{
    public abstract partial class BaseModule
    {
        /// <summary>溅射范围，仅在群体攻击时生效</summary>
        [NonSerialized] private int _splashRadius;
        /// <summary>攻击力</summary>
        [NonSerialized] public int _attackValue;
        /// <summary>攻击冷却时间</summary>
        [NonSerialized] public float _attackCD = 0f;
        /// <summary>攻击速度</summary>
        [NonSerialized] public float _attackSpeed = 1.0f;
        /// <summary>攻击类型（物理，魔法）</summary>
        [NonSerialized] public DamageType _damageType;
        /// <summary>是否可以攻击</summary>
        [NonSerialized] public bool _canAttack = true;
        /// <summary>生命值</summary>
        [NonSerialized] public int _health = 100;
        /// <summary>攻击目标数量（单体，群体，溅射）</summary>
        [NonSerialized] public TargetCount _targetCount;
        /// <summary>攻击目标移动类型（静止敌人，移动敌人）</summary>
        [NonSerialized] public TargetMovement _targetMovement;

        public int SplashRadius
        {
            get => _targetCount == TargetCount.SplashAttack ? _splashRadius : 0;
            set => _splashRadius = _targetCount == TargetCount.SplashAttack ? value : 0;
        }
    }
}