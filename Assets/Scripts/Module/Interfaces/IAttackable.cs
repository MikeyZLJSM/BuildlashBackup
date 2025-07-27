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
        ModuleParameters GetAttackParameters();
        void ExecuteAttack(GameObject target);
    }
}