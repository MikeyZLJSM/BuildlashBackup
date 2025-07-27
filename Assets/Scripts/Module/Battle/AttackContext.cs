using System.Collections.Generic;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 攻击上下文类，用于传递攻击过程中的信息
    /// </summary>
    public class AttackContext
    {
        public BaseModule sourceModule;
        
        public GameObject target;
        
        public AttackParameters parameters;
        
        public Vector3 impactPoint;
        
        public AttackContext(BaseModule sourceModule, GameObject target, AttackParameters parameters)
        {
            this.sourceModule = sourceModule;
            this.target = target;
            this.parameters = parameters;
            
            // 默认击中点为目标位置
            if (target)
            {
                impactPoint = target.transform.position;
            }
        }
        
        public void SetImpactPoint(Vector3 point)
        {
            impactPoint = point;
        }
    }
} 