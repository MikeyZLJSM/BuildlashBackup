using System.Collections.Generic;
using UnityEngine;


// TODO : 完善攻击
namespace Module.Interfaces
{
    public interface IAttackable
    {
        // 执行攻击
        bool Attack(GameObject target);

        // 执行范围攻击（用于群体攻击和溅射攻击）
        bool AttackMultiple(List<GameObject> targets);

        // 检查是否可以攻击指定目标
        bool CanAttackTarget(GameObject target);

        // 获取攻击范围内的目标
        List<GameObject> GetTargetsInRange();

        // 计算对目标的伤害值
        int CalculateDamage(GameObject target);

        // 检查攻击冷却是否完成
        bool IsAttackReady();

        // 开始攻击冷却
        void StartAttackCooldown();

        // 获取攻击范围
        float GetAttackRange();

        // 设置攻击目标
        void SetTarget(GameObject target);

        // 清除当前目标
        void ClearTarget();

        // 获取当前目标
        GameObject GetCurrentTarget();
    }
}