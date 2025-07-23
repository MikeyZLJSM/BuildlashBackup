using System.Collections.Generic;
using Module.Battle;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.Sphere
{
    public class CrystalBall : NormalSphere, IAttackable
    {
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private GameObject _bulletPrefab; // 子弹预制体
        [SerializeField] private float _bulletSpeed = 10f; // 子弹飞行速度
        [SerializeField] private Transform _firePoint; // 发射点
        
        
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化攻击参数
            _attackParameters = new AttackParameters
            {
                targetMovementType = TargetMovement.Any,
                targetCount = TargetCount.SingleEnemy,
                damageType = DamageType.Magical,
                attackAttribute = AttackAttribute.None,
                damage = 8,
                attackSpeed = 1.0f,
                attackRange = _attackRange,
                bulletSpeed = _bulletSpeed,
                bulletPrefab = _bulletPrefab
            };
            
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
        
        

        public bool CanAttack()
        {
            return _canAttack;
        }

        public void StartAttackCD()
        {
            _canAttack = false;
            _attackCD = 1f / _attackParameters.attackSpeed;
        }

        public List<GameObject> GetTargetsInRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取攻击范围内的所有敌人
            Collider[] colliders = Physics.OverlapSphere(
                transform.position, 
                _attackRange, 
                LayerMask.GetMask("Enemy")
            );
            
            // 添加找到的敌人到目标列表
            foreach (var enemyCollider in colliders)
            {
                if (enemyCollider.TryGetComponent<Enemy.BaseEnemy>(out _))
                {
                    targets.Add(enemyCollider.gameObject);
                }
            }
            
            return targets;
        }
        
        public AttackParameters GetAttackParameters()
        {
            // 返回攻击参数的副本，以防被修改
            return _attackParameters.Clone();
        }
        
        public void ExecuteAttack(GameObject target)
        {
            if (!_bulletPrefab || !target)
            {
                Debug.LogWarning("子弹预制体或目标为空，无法发射子弹");
                return;
            }
            
            // 创建攻击上下文
            AttackContext context = new AttackContext(this, target, GetAttackParameters());
            
            // 调用子弹管理器生成子弹
            Controllers.Battle.BulletManager.Instance.SpawnBullet(context);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
