using System.Collections.Generic;
using Module.Battle;
using UnityEngine;

namespace Module.Interfaces
{
    public interface IAttackable 
    {
        bool CanAttack();
        void StartAttackCD();
        List<GameObject> GetTargetsInRange();
        AttackParameters GetAttackParameters();
        void ExecuteAttack(GameObject target);
    }
}