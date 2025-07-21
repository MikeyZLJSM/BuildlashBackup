using System;
using System.Collections.Generic;
using UnityEngine;
using Module;
using Module.Enums;
using Module.Interfaces;

namespace Controllers
{
    public partial class ModulesManager : MonoBehaviour
    {
        
        public void Update()
        {
            // 更新所有模块的攻击逻辑
            UpdateModuleAttacks();
        }
        
        

        /// <summary> 更新所有模块的攻击逻辑 </summary>
        private void UpdateModuleAttacks()
        {
            foreach (ModuleInfo moduleInfo in assembledModules)
            {
                if (moduleInfo.module is not null || !moduleInfo.isAttackModule) continue;

                // 检查模块是否实现了攻击接口
                if (moduleInfo.module is IAttackable attackableModule)
                {
                    UpdateModuleCooldown(moduleInfo.module);

                    // 如果可以攻击，执行攻击逻辑
                    if (moduleInfo.module._canAttack && attackableModule.IsAttackReady())
                        ExecuteModuleAttack(attackableModule, moduleInfo);
                }
            }
        }

        /// <summary> 更新模块冷却时间 </summary>
        /// <param name = "module" > 模块 </param>
        private void UpdateModuleCooldown(BaseModule module)
        {
            if (module._attackCD > 0)
            {
                module._attackCD -= Time.deltaTime;
                if (module._attackCD <= 0) module._canAttack = true;
            }
        }

        /// <summary> 执行模块攻击 </summary>
        /// <param name = "attackableModule" > 可攻击的模块 </param>
        /// <param name = "moduleInfo" > 模块信息 </param>
        private void ExecuteModuleAttack(IAttackable attackableModule, ModuleInfo moduleInfo)
        {
            // 获取攻击范围内的目标
            List<GameObject> targets = attackableModule.GetTargetsInRange();

            if (targets != null && targets.Count > 0)
            {
                bool attackExecuted = false;

                // 根据攻击目标数量类型执行不同的攻击
                switch (moduleInfo.module._targetCount)
                {
                    case TargetCount.SingleEnemy:
                        // 单体攻击，攻击第一个目标
                        if (attackableModule.CanAttackTarget(targets[0]))
                        {
                            attackExecuted = attackableModule.Attack(targets[0]);
                            if (showDebugInfo && attackExecuted)
                                Debug.Log($"[{moduleInfo.moduleName}] 单体攻击目标: {targets[0].name}");
                        }

                        break;

                    case TargetCount.MultipleEnemies:
                        // 群体攻击
                        attackExecuted = attackableModule.AttackMultiple(targets);
                        if (showDebugInfo && attackExecuted)
                            Debug.Log($"[{moduleInfo.moduleName}] 群体攻击 {targets.Count} 个目标");
                        break;

                    case TargetCount.SplashAttack:
                        // 溅射攻击
                        attackExecuted = attackableModule.AttackMultiple(targets);
                        if (showDebugInfo && attackExecuted)
                            Debug.Log(
                                $"[{moduleInfo.moduleName}] 溅射攻击 {targets.Count} 个目标，溅射范围: {moduleInfo.module.SplashRadius}");
                        break;
                }

                // 如果攻击成功，开始冷却
                if (attackExecuted)
                {
                    attackableModule.StartAttackCooldown();
                    moduleInfo.module._canAttack = false;
                    moduleInfo.module._attackCD = 1f / moduleInfo.module._attackSpeed;
                }
            }
        }

    }
}