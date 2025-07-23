using UnityEngine;

namespace Enemy.Interfaces
{
    /// <summary>移动接口，定义敌人的移动行为</summary>
    public interface IMovable
    {
        /// <summary>执行移动逻辑</summary>
        /// <param name="enemyTransform">敌人的Transform组件</param>
        /// <param name="enemyRigidbody">敌人的Rigidbody组件</param>
        /// <param name="moveSpeed">移动速度</param>
        void Move(Transform enemyTransform, Rigidbody enemyRigidbody, float moveSpeed);
        
        /// <summary>设置移动目标</summary>
        /// <param name="target">目标Transform</param>
        void SetTarget(Transform target);
        
        /// <summary>获取当前目标</summary>
        /// <returns>当前目标Transform</returns>
        Transform GetTarget();
    }
}
