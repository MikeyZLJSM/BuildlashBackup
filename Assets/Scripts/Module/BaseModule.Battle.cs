using System;
using Module.Battle;
using Module.Enums;
using UnityEngine;

namespace Module
{
    public abstract partial class BaseModule
    {
        // 战斗参数
        [SerializeField]public AttackParameters attackParameters;
        
        [SerializeField] public float attackCd = 1.0f; 
        [SerializeField] public bool canAttack = true;
    }
}