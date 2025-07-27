using Enemy;
using Module.Battle;
using Module.Interfaces;
using UnityEngine;

namespace Controllers.Battle
{
    /// <summary>
    /// 普通攻击属性，无特殊效果
    /// </summary>
    public class NormalAttribute : IAttackAttribute
    {
        public void ApplyAttribute(AttackContext context)
        {
            // 获取目标敌人
            if (context.target && context.target.TryGetComponent<BaseEnemy>(out var enemy))
            {
                // 直接造成伤害
                enemy.TakeDamage(context.parameters.damage);
                
                Debug.Log($"{context.sourceModule.name} 对 {enemy.name} 造成了 {context.parameters.damage} 点{context.parameters.damageType}伤害");
            }
        }
    }
} 