using System.Collections.Generic;
using Enemy;
using Module.Interfaces;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 溅射攻击属性，以命中
    /// </summary>
    public class SplashAttribute : IAttackAttribute
    {
        public void ApplyAttribute(AttackContext context)
        {
            // 主目标造成直接命中伤害
            if (context.target && context.target.TryGetComponent<BaseEnemy>(out var targetEnemy))
            {
                targetEnemy.TakeDamage(context.parameters.damage);
                Debug.Log($"{context.sourceModule.name} 对主目标 {targetEnemy.name} 造成了 {context.parameters.damage} 点{context.parameters.damageType}伤害");
            }
            
            float splashRadius = context.parameters.SplashRadius;
            if (splashRadius <= 0f)
            {
                splashRadius = 3f;
                Debug.LogWarning("溅射半径未设置，使用默认值3");
            }
            
            // 获取命中点
            Vector3 impactPoint = context.impactPoint;
            
            Collider[] colliders = Physics.OverlapSphere(impactPoint, splashRadius, 1 << 8); 
            
            HashSet<GameObject> processedEnemies = new HashSet<GameObject>();
            if (context.target)
            {
                //主目标不受到溅射伤害
                processedEnemies.Add(context.target); 
            }
            
            foreach (var collider in colliders)
            {
                GameObject enemyObject = collider.gameObject;
                
                if (processedEnemies.Contains(enemyObject))
                {
                    continue;
                }
                
                if (enemyObject.TryGetComponent<BaseEnemy>(out var enemy))
                {
                    // 根据伤害系数计算伤害
                    float distance = Vector3.Distance(impactPoint, enemyObject.transform.position);
                    float damageFactor = 1f - (distance / splashRadius); 
                    damageFactor = Mathf.Clamp(damageFactor, 0.5f, 1f); // 保底百分之30伤害
                    
                    // 计算溅射伤害
                    int splashDamage = Mathf.RoundToInt(context.parameters.damage * damageFactor);
                    
                    enemy.TakeDamage(splashDamage);
                    
                    Debug.Log($"{context.sourceModule.name} 对范围内敌人 {enemy.name} 造成了 {splashDamage} 点溅射伤害 (距离: {distance:F2}, 系数: {damageFactor:F2})");
                    
                    processedEnemies.Add(enemyObject);
                }
            }
            
        }
    }
} 