using Enemy.Interfaces;
using UnityEngine;

namespace Enemy.AttackStrategies
{
    /// <summary>近战攻击策略</summary>
    public class MeleeAttack : IAttackable
    {
        private float _attackRange = 2f;
        private float _cooldownTime = 1f;
        
        /// <summary>执行近战攻击</summary>
        public bool Attack(Transform attacker, Transform target, int attackPower)
        {
            if (!CanAttack(attacker, target)) return false;
            
            // 检查目标是否有BaseModule组件（假设这是攻击目标）
            var targetModule = target.GetComponent<Module.BaseModule>();
            if (targetModule != null)
            {
                // 通过BattleManager处理伤害
                if (Controllers.BattleManager.Instance != null)
                {
                    Controllers.BattleManager.Instance.DamagePlayer(attackPower, targetModule);
                    Debug.Log($"近战攻击命中目标，造成 {attackPower} 点伤害");
                    return true;
                }
                else
                {
                    Debug.LogWarning("BattleManager实例未找到，无法处理伤害");
                }
            }
            
            return false;
        }
        
        /// <summary>检查是否可以近战攻击（忽略Y轴高度差）</summary>
        public bool CanAttack(Transform attacker, Transform target)
        {
            if (attacker == null || target == null) return false;
            
            // 只计算水平距离，忽略Y轴
            Vector3 attackerPos = new Vector3(attacker.position.x, 0, attacker.position.z);
            Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);
            float distance = Vector3.Distance(attackerPos, targetPos);
            
            return distance <= _attackRange;
        }
        
        /// <summary>获取攻击冷却时间</summary>
        public float GetCooldownTime() => _cooldownTime;
        
        /// <summary>获取攻击范围</summary>
        public float GetAttackRange() => _attackRange;
        
        /// <summary>设置近战攻击参数</summary>
        public void SetAttackParameters(float range, float cooldown)
        {
            _attackRange = range;
            _cooldownTime = cooldown;
        }
    }
}
