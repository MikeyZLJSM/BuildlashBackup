using System;
using System.Collections.Generic;
using Module;
using UnityEngine;

namespace Controllers.Battle
{
    /// <summary>战斗管理器，统一管理所有敌人的逻辑更新，我方模块的统一属性</summary>
    public partial class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public float PlayerMaxHealth;
        public float PlayerCurrentHealth;
        
        /// <summary>初始化单例</summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void OnEnable()
        {
            CaculateMaxHealth();
            PlayerCurrentHealth = PlayerMaxHealth;
            UIController.Instance.UpdateHealthText(PlayerCurrentHealth,PlayerMaxHealth );
            
            InitializeModuleBattleSystem();
        }

        private void Update()
        {
            UpdateEnemyLogic();
            
            UpdateModuleBattle();
        }

        /// <summary>计算玩家最大血量</summary>
        /// <returns>最大血量值</returns>
        public float CaculateMaxHealth()
        {
            PlayerMaxHealth = 0f;
            foreach( ModulesManager.ModuleInfo moduleInfo in  ModulesManager.Instance.GetAllModulesInfo())
            {
                PlayerMaxHealth += moduleInfo.module._health;
            }
            return PlayerMaxHealth;
        }
        
        /// <summary>玩家死亡处理</summary>
        private void OnPlayerDeath()
        {
            Debug.Log("玩家死亡!");
            // 可以在这里添加游戏结束逻辑
        }
        
        /// <summary>强制刷新血量显示</summary>
        public void RefreshHealthDisplay()
        {
            CaculateMaxHealth();
            PlayerCurrentHealth = Mathf.Min(PlayerCurrentHealth, PlayerMaxHealth);
            UpdateHealthDisplay();
        }
        
        /// <summary>更新血量UI显示</summary>
        private void UpdateHealthDisplay()
        {
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateHealthText(PlayerCurrentHealth, PlayerMaxHealth);
            }
            else
            {
                Debug.LogWarning("UIController实例未找到，无法更新血量显示");
            }
        }
        
        public void OnDisable()
        {
            ClearAllEnemies();
        }
    }
}
