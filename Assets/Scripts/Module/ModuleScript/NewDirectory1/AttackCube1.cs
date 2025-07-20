using System.Collections.Generic;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.NewDirectory1
{
    public class AttackCube1: NormalCube,IAttackable
    {

        public bool Attack(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public bool AttackMultiple(List<GameObject> targets)
        {
            throw new System.NotImplementedException();
        }

        public bool CanAttackTarget(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public List<GameObject> GetTargetsInRange()
        {
            throw new System.NotImplementedException();
        }

        public int CalculateDamage(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public bool IsAttackReady()
        {
            throw new System.NotImplementedException();
        }

        public void StartAttackCooldown()
        {
            throw new System.NotImplementedException();
        }

        public float GetAttackRange()
        {
            throw new System.NotImplementedException();
        }

        public void SetTarget(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        public void ClearTarget()
        {
            throw new System.NotImplementedException();
        }

        public GameObject GetCurrentTarget()
        {
            throw new System.NotImplementedException();
        }
    }
}