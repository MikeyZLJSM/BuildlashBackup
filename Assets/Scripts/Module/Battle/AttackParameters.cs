using System;
using Module.Enums;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 攻击参数类，封装所有攻击相关参数
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "AttackParameters", menuName = "Module/AttackParameters", order = 1)]
    public class AttackParameters
    {
        public string moduleName;
        public TargetLockType targetLockType = TargetLockType.Nearest;
        public int bulletCount = 1;
        public int targetCount = 1;
        public DamageType damageType = DamageType.Physical;
        public AttackAttribute attackAttribute = AttackAttribute.None;
        public int damage = 10;
        public float attackSpeed = 1.0f;
        public float attackRange = 10.0f;
        public float attackCD { get;set; } // 攻击冷却时间，单位秒
        public bool canAttack { get; set; }
        public float bulletSpeed = 10.0f;
        public GameObject bulletPrefab;
        
        // 溅射半径（溅射攻击时有效）
        private float splashRadius;
        // 伤害频率 （持续攻击时有效）
        private float tickInterval;
        
        public float SplashRadius
        {
            get => attackAttribute == AttackAttribute.Splash ? splashRadius : 0;
            set => splashRadius = value;
        }
        
        public float TickInterval
        {
            get => attackAttribute == AttackAttribute.Continuous ? tickInterval : 0;
            set => tickInterval = value;
        }
        
        /// <summary>
        /// 创建攻击参数的副本
        /// </summary>
        public AttackParameters Clone()
        {
            return new AttackParameters
            {
                targetLockType = this.targetLockType,
                bulletCount = this.bulletCount,
                targetCount = this.targetCount,
                damageType = this.damageType,
                attackAttribute = this.attackAttribute,
                damage = this.damage,
                attackSpeed = this.attackSpeed,
                attackRange = this.attackRange,
                splashRadius = this.splashRadius,
                bulletSpeed = this.bulletSpeed,
                bulletPrefab = this.bulletPrefab
            };
        }
    }
} 