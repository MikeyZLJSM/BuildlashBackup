using System;
using System.Collections.Generic;
using UnityEngine;
using Module;
using Module.Battle;
using Module.Enums;
using Module.Interfaces;

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
                UpdateModuleCD(module);
            }
            
            // 处理攻击模块的逻辑
            foreach (var module in _battleModules)
            {
                if (module is IAttackable attackable)
                {
                    ProcessModuleAttack(module, attackable);
                }
            }
        }
        
        // 更新模块冷却时间
        private void UpdateModuleCD(BaseModule module)
        {
            if (module._attackCD >= 0)
            {
                module._attackCD -= Time.deltaTime;
                if (module._attackCD <= 0)
                {
                    module._canAttack = true;
                }
            }
        }
        
        // 处理模块攻击逻辑
        private void ProcessModuleAttack(BaseModule module, IAttackable attackable)
        {
            // 检查模块是否可以攻击
            if (!attackable.CanAttack())
            {
                return;
            }

            // 获取原始攻击参数
            AttackParameters parameters = attackable.GetAttackParameters();
            
            // 获取攻击范围内的目标
            List<GameObject> targets = attackable.GetTargetsInRange();
            if (targets == null || targets.Count == 0)
            {
                return;
            }
            
            
            switch (parameters.targetCount)
            {
                case TargetCount.SingleEnemy:
                    attackable.ExecuteAttack(targets[0]);
                    break;
                    
                case TargetCount.MultipleEnemies:
                    foreach (var target in targets)
                    {
                        attackable.ExecuteAttack(target);
                    }
                    break;
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
