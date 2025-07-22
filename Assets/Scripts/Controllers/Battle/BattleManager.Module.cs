using System;
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using Module;
using Module.Enums;
using Module.Interfaces;

namespace Controllers.Battle
{
    public partial class BattleManager
    {
        // 所有具有战斗行为的模块列表
        private List<BaseModule> _battleModules = new List<BaseModule>();
        
        [SerializeField] private bool _showAttackRanges = false;
        
        // 初始化模块战斗系统
        public void InitializeModuleBattleSystem()
        {
            // 从ModulesManager获取所有已组装的模块
            var moduleInfos = ModulesManager.Instance.GetAllModulesInfo();
            _battleModules.Clear();
            
            // TODO：目前ModuleInfo的属性只能区别攻击模块，后续需要判断所有战斗模块的种类，根据种类来处理对应的战斗逻辑
            foreach (var battleModule in moduleInfos)
            {
                if (battleModule.module != null && battleModule.isAttackModule)
                {
                    _battleModules.Add(battleModule.module);
                }
            }
            
            Debug.Log($"初始化模块战斗系统，共加载 {_battleModules.Count} 个模块");
        }
        
        // 更新所有模块的战斗逻辑
        private void UpdateModuleBattleLogic()
        {
            // 更新所有模块的冷却时间
            foreach (var module in _battleModules)
            {
                UpdateModuleCooldown(module);
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
        private void UpdateModuleCooldown(BaseModule module)
        {
            if (module._attackCD > 0)
            {
                module._attackCD -= Time.deltaTime;
                if (module._attackCD <= 0)
                {
                    module._canAttack = true;
                }
            }
        }
        
        // 处理模块攻击逻辑
        private void ProcessModuleAttack(BaseModule module, IAttackable attackingModule)
        {
            // 检查模块是否可以攻击
            if (!attackingModule.CanAttack())
            {
                return;
            }
            
            // 获取攻击范围内的目标
            List<GameObject> targets = attackingModule.GetTargetsInRange();
            if (targets == null || targets.Count == 0)
            {
                return;
            }
            
            // 根据攻击类型执行不同的攻击逻辑
            AttackType attackType = attackingModule.GetAttackType();
            
            // 根据模块的目标数量选择攻击方式
            switch (module._targetCount)
            {
                case TargetCount.SingleEnemy:
                    // 单体攻击，只攻击第一个目标
                    ExecuteAttack(module, targets[0], attackingModule);
                    break;
                    
                case TargetCount.MultipleEnemies:
                    // 群体攻击，攻击所有目标
                    foreach (var target in targets)
                    {
                        ExecuteAttack(module, target, attackingModule);
                    }
                    break;
            }
            
            // 攻击后开始冷却
            attackingModule.StartAttackCD();
        }
        
        // 执行单体攻击
        private void ExecuteAttack(BaseModule module, GameObject target, IAttackable attackingModule)
        {
            if (target.TryGetComponent<BaseEnemy>(out var enemy))
            {
                attackingModule.Fire(target);
                
                // 可以在这里添加攻击特效
                Debug.Log($"{module.name} 对 {enemy.name} 造成了 {module._attackValue} 点{module._damageType}伤害");
            }
        }
        
        public void UpdateModuleLogic()
        {
            UpdateModuleBattleLogic();
        }
    }
}
