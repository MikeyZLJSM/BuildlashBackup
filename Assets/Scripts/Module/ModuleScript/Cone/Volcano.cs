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
                damage = 8,
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
        
        public override void ExecuteAttack(GameObject target)
        {
            if (!_bulletPrefab || !target)
            {
                Debug.LogWarning("子弹预制体或目标为空，无法发射子弹");
                return;
            }
            
            AttackContext context = new AttackContext(this, target, GetAttackParameters());
            
            StartCoroutine(Controllers.Battle.BulletManager.Instance.SpawnBullets(context));
        }
    }
}
