using Enemy.Interfaces;
using UnityEngine;

namespace Enemy.MovementStrategies
{
    /// <summary>直线移动策略，敌人直接朝目标移动，到达攻击范围时停止</summary>
    public class StraightMovement : IMovable
    {
        private Transform target;
        private float attackRange = 2f; // 攻击范围，用于判断是否停止移动
        
        /// <summary>执行直线移动</summary>
        /// <param name="enemyTransform">敌人的Transform组件</param>
        /// <param name="enemyRigidbody">敌人的Rigidbody组件</param>
        /// <param name="moveSpeed">移动速度</param>
        public void Move(Transform enemyTransform, Rigidbody enemyRigidbody, float moveSpeed)
        {
            if (target == null || enemyTransform == null || enemyRigidbody == null) return;
            
            // 计算目标在水平面上的投影位置（保持敌人的Y坐标）
            Vector3 targetPosition = new Vector3(target.position.x, enemyTransform.position.y, target.position.z);
            float distanceToTarget = Vector3.Distance(enemyTransform.position, targetPosition);
            
            // 如果距离目标超过攻击范围，则继续移动
            if (distanceToTarget > attackRange)
            {
                Vector3 direction = (targetPosition - enemyTransform.position).normalized;
                Vector3 newPosition = enemyTransform.position + direction * (moveSpeed * Time.fixedDeltaTime);
                enemyRigidbody.MovePosition(newPosition);
                
                // 让敌人面向目标（水平方向）
                if (direction != Vector3.zero)
                {
                    enemyTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
            else
            {
                // 在攻击范围内时停止移动，但仍然面向目标（水平方向）
                Vector3 direction = (targetPosition - enemyTransform.position).normalized;
                if (direction != Vector3.zero)
                {
                    enemyTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        
        /// <summary>设置移动目标</summary>
        /// <param name="target">目标Transform</param>
        public void SetTarget(Transform target)
        {
            this.target = target;
        }
        
        /// <summary>获取当前目标</summary>
        /// <returns>当前目标Transform</returns>
        public Transform GetTarget()
        {
            return target;
        }
        
        /// <summary>设置攻击范围，用于判断何时停止移动</summary>
        /// <param name="range">攻击范围</param>
        public void SetAttackRange(float range)
        {
            attackRange = range;
        }
        
        /// <summary>获取当前设置的攻击范围</summary>
        /// <returns>攻击范围</returns>
        public float GetAttackRange()
        {
            return attackRange;
        }
    }
}
