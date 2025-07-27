using Module.Battle;
using Module.Enums;
using UnityEngine;

namespace Module.ModuleScript.Cone
{
    public class Volcano : NormalCone
    {
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化攻击参数
            AttackParameters = new AttackParameters
            {
                targetLockType = TargetLockType.Nearest,
                bulletCount = 1,
                targetCount = 4,
                damageType = DamageType.Magical,
                attackAttribute = AttackAttribute.None,
                damage = 6,
                attackSpeed = 0.25f,
                attackRange = 20f,
                bulletSpeed = 15f,
                bulletPrefab = _bulletPrefab
            };
            
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
        
    }
}
