using Enemy;
using Module.Interfaces;
using UnityEngine;

namespace Module.Battle
{
    public class ContinuousAttribute : IAttackAttribute
    {
        public void ApplyAttribute(AttackContext context)
        {
            if (context.target && context.target.TryGetComponent<BaseEnemy>(out var enemy))
            {
                enemy.TakeDamage(context.parameters.damage);
            }
        }
    }
}
