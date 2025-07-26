using System;
using System.Collections.Generic;
using UnityEngine;
using Module;
using Module.Battle;

namespace Controllers.Battle
{
    public partial class BattleManager
    {
        // 具有战斗行为的模块列表
        private List<BaseModule> _battleModules = new List<BaseModule>();
        
        public void InitializeModuleBattleSystem()
        {
            // 从ModulesManager获取所有已组装的模块
            var moduleInfos = ModulesManager.Instance.GetAllModulesInfo();
            _battleModules.Clear();
            
            foreach (var moduleInfo in moduleInfos)
            {
                if (moduleInfo.module != null && moduleInfo.isAttackModule)
                {
                    _battleModules.Add(moduleInfo.module);
                }
            }
            
            Debug.Log($"初始化模块战斗系统，共加载 {_battleModules.Count} 个模块");
        }
        
        // 更新所有模块的战斗逻辑
        private void UpdateModuleBattleLogic()
        {
            foreach (var module in _battleModules)
            {
                UpdateAttackModuleCD(module);
            }
            
            // 处理攻击模块的逻辑
            foreach (var module in _battleModules)
            {
                if (module is BaseAttackModule attackable)
                {
                    ProcessModuleAttack(module, attackable);
                }
            }
        }
        
        // 更新模块冷却时间
        private void UpdateAttackModuleCD(BaseModule module)
        {
            if (module is not BaseAttackModule attackable || !(attackable.AttackParameters.attackCD >= 0)) return;
            attackable.AttackParameters.attackCD -= Time.deltaTime;
            if (attackable.AttackParameters.attackCD <= 0)
            {
                attackable.AttackParameters.canAttack = true;
            }
        }
        
        // 处理模块攻击逻辑
        private void ProcessModuleAttack(BaseModule module, BaseAttackModule attackable)
        {
            if (!attackable.CanAttack())
            {
                return;
            }

            // 获取原始攻击参数，使用AttackModifier装饰器修改
            AttackParameters parameters = attackable.GetAttackParameters();
            var newParameters = AttackModifier(parameters);
            
            List<GameObject> targets = attackable.GetTargetsInRange();
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            if (parameters.targetCount == 1)
            {
                attackable.ExecuteAttack(targets[0]);
            }
            else if(parameters.targetCount > 1)
            {
                if (targets.Count < parameters.targetCount)
                {
                    foreach (var target in targets)
                    {
                        attackable.ExecuteAttack(target);
                    }
                }
                else
                {
                    for (int i = 0; i < parameters.targetCount; i++)
                    {
                        attackable.ExecuteAttack(targets[i]);
                    }
                }
            }
            
            
            attackable.StartAttackCD();
        }
        

        private AttackParameters AttackModifier(AttackParameters  parameters)
        {
            return parameters;
        }
        
        private void UpdateModuleBattle()
        {
            UpdateModuleBattleLogic();
        }
        
    }
}
