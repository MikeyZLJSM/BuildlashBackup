using System.Collections.Generic;
using Enemy;
using Module.Battle;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.Sphere
{
    public class CrystalBall : Sphere
    {
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化攻击参数
            AttackParameters = new AttackParameters
            {
                targetLockType = TargetLockType.Nearest,
                bulletCount = 1,
                targetCount = 1,
                damageType = DamageType.Magical,
                attackAttribute = AttackAttribute.None,
                damage = 8,
                attackSpeed = 1.0f,
                attackRange = 20f,
                bulletSpeed = 10f,
                bulletPrefab = _bulletPrefab
            };
            
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
        
    }
}