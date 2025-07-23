using UnityEngine;

namespace Enemy.Interfaces
{
    /// <summary>攻击接口，定义敌人的攻击行为</summary>
    public interface IAttackable
    {
        /// <summary>执行攻击逻辑</summary>
        /// <param name="attacker">攻击者的Transform</param>
        /// <param name="target">攻击目标</param>
        /// <param name="attackPower">攻击力</param>
        /// <returns>是否成功执行攻击</returns>
        bool Attack(Transform attacker, Transform target, int attackPower);
        
        /// <summary>检查是否可以攻击目标</summary>
        /// <param name="attacker">攻击者的Transform</param>
        /// <param name="target">攻击目标</param>
        /// <returns>是否可以攻击</returns>
        bool CanAttack(Transform attacker, Transform target);
        
        /// <summary>获取攻击冷却时间</summary>
        /// <returns>冷却时间（秒）</returns>
        float GetCooldownTime();
        
        /// <summary>获取攻击范围</summary>
        /// <returns>攻击范围</returns>
        float GetAttackRange();
    }
}
