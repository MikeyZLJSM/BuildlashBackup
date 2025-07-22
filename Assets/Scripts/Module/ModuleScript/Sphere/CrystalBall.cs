using System.Collections.Generic;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.Sphere
{
    public class CrystalBall : NormalSphere, IAttackable
    {
        [SerializeField] private float _attackRange = 5f;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 临时设置水晶球的战斗属性
            _attackValue = 8;                            
            _damageType = DamageType.Magical;            
            _targetCount = TargetCount.SingleEnemy;      
            _attackSpeed = 1.0f;                         
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
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
