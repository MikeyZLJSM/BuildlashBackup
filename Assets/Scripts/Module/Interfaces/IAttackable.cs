using System.Collections.Generic;
using UnityEngine;


namespace Module.Interfaces
{
    public interface IAttackable
    {
        /// <summary>执行攻击</summary>
        bool Attack(GameObject target);

        /// <summary>执行范围攻击（用于群体攻击和溅射攻击）</summary>
        bool AttackMultiple(List<GameObject> targets);

        /// <summary>检查是否可以攻击指定目标</summary>
        bool CanAttackTarget(GameObject target);

        /// <summary>获取攻击范围内的目标</summary>
        List<GameObject> GetTargetsInRange();

        /// <summary>计算对目标的伤害值</summary>
        int CalculateDamage(GameObject target);

        /// <summary>检查攻击冷却是否完成</summary>
        bool IsAttackReady();

        /// <summary>开始攻击冷却</summary>
        void StartAttackCooldown();

        /// <summary>获取攻击范围</summary>
        float GetAttackRange();

        /// <summary>设置攻击目标</summary>
        void SetTarget(GameObject target);

        /// <summary>清除当前目标</summary>
        void ClearTarget();

        /// <summary>获取当前目标</summary>
        GameObject GetCurrentTarget();
    }
}