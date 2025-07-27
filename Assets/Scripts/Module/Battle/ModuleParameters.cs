using System;
using Module.Enums;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 战斗参数类，封装所有战斗相关参数
    /// </summary>
    [Serializable]
    public class ModuleParameters
    {
        /// <summary>模块名称</summary>
        [SerializeField] public string moduleName = "DefaultModule";
        
        /// <summary>目标锁定类型</summary>
        [SerializeField] public TargetLockType targetLockType = TargetLockType.Nearest;
        /// <summary>每次攻击发射的子弹数量</summary>
        [SerializeField] public int bulletCount = 1;
        /// <summary>每次攻击可以同时攻击的目标数量</summary>
        [SerializeField] public int targetCount = 1;
        /// <summary>伤害类型</summary>
        [SerializeField] public DamageType damageType = DamageType.Physical;
        /// <summary>攻击属性，如普通攻击、溅射攻击等特殊效果</summary>
        [SerializeField] public AttackAttribute attackAttribute = AttackAttribute.None;
        /// <summary>基础攻击伤害值</summary>
        [SerializeField] public int damage = 10;
        /// <summary>攻击速度，每秒攻击次数</summary>
        [SerializeField] public float attackSpeed = 1.0f;
        /// <summary>子弹飞行速度</summary>
        [SerializeField] public float bulletSpeed = 10.0f;
        /// <summary>子弹预制体引用</summary>
        [SerializeField] public GameObject bulletPrefab;
        /// <summary>模块生命值</summary>
        [SerializeField] public float health = 50.0f; 
        
        
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
        public ModuleParameters Clone()
        {
            return new ModuleParameters
            {
                targetLockType = this.targetLockType,
                bulletCount = this.bulletCount,
                targetCount = this.targetCount,
                damageType = this.damageType,
                attackAttribute = this.attackAttribute,
                damage = this.damage,
                attackSpeed = this.attackSpeed,
                splashRadius = this.splashRadius,
                bulletSpeed = this.bulletSpeed,
                bulletPrefab = this.bulletPrefab
            };
        }
    }
}
