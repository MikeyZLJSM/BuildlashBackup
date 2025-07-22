using System.Collections.Generic;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.Sphere
{
    public class CrystalBall : NormalSphere, IAttackable
    {
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private GameObject _bulletPrefab; 
        [SerializeField] private float _bulletSpeed = 10f; 
        [SerializeField] private Transform _firePoint; 
        
        protected override void Awake()
        {
            base.Awake();
            
            // 设置水晶球的战斗属性
            _attackValue = 8;                            
            _damageType = DamageType.Magical;            
            _targetCount = TargetCount.SingleEnemy;      
            _attackSpeed = 1.0f;
            
            // 如果没有指定发射点，默认使用自身位置
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
        
        public AttackType GetAttackType()
        {
            return AttackType.Periodic;
        }

        public bool CanAttack()
        {
            return _canAttack;
        }

        public void StartAttackCD()
        {
            _canAttack = false;
            _attackCD = 1f / _attackSpeed;
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
        
        public void Fire(GameObject target)
        {
            if (_bulletPrefab == null || target == null)
            {
                Debug.LogWarning("子弹预制体或目标为空");
                return;
            }
            
            // 统一调用子弹管理器生成子弹
            Controllers.Battle.BulletManager.Instance.SpawnAndFireBullet(
                _bulletPrefab,
                _firePoint.position,
                target.transform,
                _attackValue,
                _damageType,
                _bulletSpeed
            );
            
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
