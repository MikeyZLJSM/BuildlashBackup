using System.Collections.Generic;
using Module.Enums;
using UnityEngine;

namespace Module.Interfaces
{
    public interface IAttackable 
    {
        AttackType GetAttackType();
        bool CanAttack();
        void StartAttackCD();
        List<GameObject> GetTargetsInRange();
        void Fire(GameObject target); // 攻击模块统一实现FireBullet，调用BulletManager发射子弹
    }
}