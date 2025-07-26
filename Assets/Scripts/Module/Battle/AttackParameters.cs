using System;
using Module.Enums;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 攻击参数类，封装所有攻击相关参数
    /// </summary>
    [Serializable]
    public class AttackParameters
    {
        [SerializeField] public TargetLockType targetLockType = TargetLockType.Nearest;
        [SerializeField] public int bulletCount = 1;
        [SerializeField] public int targetCount = 1;
        [SerializeField] public DamageType damageType = DamageType.Physical;
        [SerializeField] public AttackAttribute attackAttribute = AttackAttribute.None;
        [SerializeField] public int damage = 10;
        [SerializeField] public float attackSpeed = 1.0f;
        [SerializeField] public float attackRange = 5.0f;
        [SerializeField] public float attackCD;
        [SerializeField] public bool canAttack;
        [SerializeField] public float bulletSpeed = 10.0f;
        [SerializeField] public GameObject bulletPrefab;
        
        // 溅射半径（仅在溅射攻击时有效）
        [SerializeField] private float splashRadius;
        
        public float SplashRadius
        {
            get => attackAttribute == AttackAttribute.Splash ? splashRadius : 0;
            set => splashRadius = value;
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