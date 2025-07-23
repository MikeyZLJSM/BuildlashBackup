using Enemy.Interfaces;
using UnityEngine;

namespace Enemy.AttackStrategies
{
    /// <summary>远程攻击策略</summary>
    /// // todo: 实现远程攻击逻辑
    public class RangedAttack : IAttackable
    {


        public bool Attack()
        {
            throw new System.NotImplementedException();
        }

        public bool Attack(Transform attacker, Transform target, int attackPower)
        {
            throw new System.NotImplementedException();
        }

        public bool CanAttack(Transform attacker, Transform target)
        {
            throw new System.NotImplementedException();
        }

        public float GetCooldownTime()
        {
            throw new System.NotImplementedException();
        }

        public float GetAttackRange()
        {
            throw new System.NotImplementedException();
        }
    }
}
