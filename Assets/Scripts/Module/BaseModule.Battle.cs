using System;
using Module.Enums;

namespace Module
{
    public abstract partial class BaseModule
    {
        // 溅射范围，仅在群体攻击时生效
        [NonSerialized] private int _splashRadius;
        [NonSerialized] public int Attack; // 攻击力
        [NonSerialized] public float AttackCooldown = 0f; // 攻击冷却时间
        [NonSerialized] public float AttackSpeed = 1.0f; // 攻击速度
        [NonSerialized] public AttackType AttackType; // 攻击类型（物理，魔法）
        [NonSerialized] public bool CanAttack = true; // 是否可以攻击
        [NonSerialized] public int Health; // 生命值
        [NonSerialized] public TargetCount TargetCount; // 攻击目标数量（单体，群体，溅射）
        [NonSerialized] public TargetMobility TargetMobility; // 攻击目标移动类型（静止敌人，移动敌人）

        public int SplashRadius
        {
            get => TargetCount == TargetCount.SplashAttack ? _splashRadius : 0;
            set => _splashRadius = TargetCount == TargetCount.SplashAttack ? value : 0;
        }
    }
    
}