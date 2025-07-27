using System.Collections.Generic;
using Enemy;
using Module.Interfaces;
using UnityEngine;

namespace Module.Battle
{
    /// <summary>
    /// 溅射攻击属性，对命中点周围区域内的敌人造成伤害
    /// </summary>
    public class SplashAttribute : IAttackAttribute
    {
        // 溅射效果预制体
        private static GameObject _splashEffectPrefab;
        
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
            
            // 创建溅射效果可视化
            CreateSplashVisualEffect(impactPoint, splashRadius);
            
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
                    // 计算距离，用于伤害衰减
                    float distance = Vector3.Distance(impactPoint, enemyObject.transform.position);
                    float damageFactor = 1f - (distance / splashRadius); // 距离越远，伤害越低
                    damageFactor = Mathf.Clamp(damageFactor, 0.3f, 1f); // 确保最低造成30%的伤害
                    
                    // 计算溅射伤害
                    int splashDamage = Mathf.RoundToInt(context.parameters.damage * damageFactor);
                    
                    enemy.TakeDamage(splashDamage);
                    
                    Debug.Log($"{context.sourceModule.name} 对范围内敌人 {enemy.name} 造成了 {splashDamage} 点溅射伤害 (距离: {distance:F2}, 系数: {damageFactor:F2})");
                    
                    processedEnemies.Add(enemyObject);
                }
            }
            
            // 可视化溅射范围（仅在编辑器中）
            #if UNITY_EDITOR
            Debug.DrawLine(impactPoint, impactPoint + Vector3.up * splashRadius, Color.red, 1f);
            Debug.DrawLine(impactPoint, impactPoint + Vector3.right * splashRadius, Color.red, 1f);
            Debug.DrawLine(impactPoint, impactPoint + Vector3.left * splashRadius, Color.red, 1f);
            Debug.DrawLine(impactPoint, impactPoint + Vector3.forward * splashRadius, Color.red, 1f);
            Debug.DrawLine(impactPoint, impactPoint + Vector3.back * splashRadius, Color.red, 1f);
            #endif
        }
        
        /// <summary>
        /// 创建溅射伤害的可视化效果
        /// </summary>
        private void CreateSplashVisualEffect(Vector3 position, float radius)
        {
            // 创建一个临时的可视化效果
            GameObject visualEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualEffect.transform.position = position;
            visualEffect.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            
            // 设置材质为半透明
            Renderer renderer = visualEffect.GetComponent<Renderer>();
            if (renderer)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(1f, 0.5f, 0f, 0.3f); // 橙色半透明
                material.SetFloat("_Mode", 3); // 设置为透明模式
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                renderer.material = material;
            }
            
            // 禁用碰撞器
            Collider collider = visualEffect.GetComponent<Collider>();
            if (collider)
            {
                collider.enabled = false;
            }
            
            // 一段时间后销毁
            GameObject.Destroy(visualEffect, 0.5f);
        }
    }
} 