using System;
using System.Collections.Generic;
using Module;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controllers
{
    /// <summary>战斗管理器，统一管理所有敌人的逻辑更新，我方模块的统一属性</summary>
    public partial class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public float PlayerMaxHealth;
        public float PlayerCurrentHealth;
        
        /// <summary>所有注册的敌人列表</summary>
        [SerializeField]private List<Enemy.BaseEnemy> registeredEnemies = new List<Enemy.BaseEnemy>();
        
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
        }

        /// <summary>每帧更新所有敌人的逻辑</summary>
        private void Update()
        {
            // 倒序遍历，防止在遍历过程中移除敌人时出现索引错误
            for (int i = registeredEnemies.Count - 1; i >= 0; i--)
            {
                if (registeredEnemies[i] == null)
                {
                    // 如果敌人被销毁，从列表中移除
                    registeredEnemies.RemoveAt(i);
                }
                else
                {
                    // 调用敌人的逻辑更新
                    registeredEnemies[i].OnBattleUpdate();
                }
            }
            
            
            if (Input.GetMouseButtonDown(0))
            {
                GenerateEnemy();
            }
        }
        
        /// <summary>注册敌人到战斗管理器</summary>
        /// <param name="enemy">要注册的敌人</param>
        public void RegisterEnemy(Enemy.BaseEnemy enemy)
        {
            if (enemy != null && !registeredEnemies.Contains(enemy))
            {
                registeredEnemies.Add(enemy);
                Debug.Log($"敌人 {enemy.name} 已注册到战斗管理器");
            }
        }
        
        
        
        /// <summary>从战斗管理器注销敌人</summary>
        /// <param name="enemy">要注销的敌人</param>
        public void UnregisterEnemy(Enemy.BaseEnemy enemy)
        {
            if (registeredEnemies.Contains(enemy))
            {
                registeredEnemies.Remove(enemy);
                Debug.Log($"敌人 {enemy.name} 已从战斗管理器注销");
            }
        }
        
        /// <summary>获取当前注册的敌人数量</summary>
        /// <returns>敌人数量</returns>
        public int GetEnemyCount()
        {
            return registeredEnemies.Count;
        }
        
        /// <summary>获取所有注册的敌人列表（只读）</summary>
        /// <returns>敌人列表的只读副本</returns>
        public List<Enemy.BaseEnemy> GetAllEnemies()
        {
            return new List<Enemy.BaseEnemy>(registeredEnemies);
        }
        
        /// <summary>清除所有注册的敌人</summary>
        public void ClearAllEnemies()
        {
            foreach (Enemy.BaseEnemy enemy in registeredEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            registeredEnemies.Clear();
        }
        
        /// <summary>计算玩家最大血量</summary>
        /// <returns>最大血量值</returns>
        public float CaculateMaxHealth()
        {
            PlayerMaxHealth = 0f;
            foreach( ModulesManager.ModuleInfo moduleInfo in  ModulesManager.Instance.GetAllModulesInfo())
            {
                PlayerMaxHealth += moduleInfo.module.Health;
            }
            return PlayerMaxHealth;
        }
        
        /// <summary>对玩家造成伤害</summary>
        /// <param name="damage">伤害值</param>
        /// <param name="targetModule">被攻击的目标模块</param>
        public void DamagePlayer(int damage, BaseModule targetModule = null)
        {
            float previousHealth = PlayerCurrentHealth;
            PlayerCurrentHealth = Mathf.Max(0, PlayerCurrentHealth - damage);
            
            // 敌人攻击时立即更新血量显示
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateHealthText(PlayerCurrentHealth, PlayerMaxHealth);
            }
            
            Debug.Log($"玩家受到 {damage} 点伤害，血量: {PlayerCurrentHealth}/{PlayerMaxHealth}");
            
            // 检查是否死亡
            if (PlayerCurrentHealth <= 0 && previousHealth > 0)
            {
                OnPlayerDeath();
            }
        }
        
        /// <summary>玩家死亡处理</summary>
        private void OnPlayerDeath()
        {
            Debug.Log("玩家死亡!");
            // 可以在这里添加游戏结束逻辑
        }
        
        /// <summary>治疗玩家</summary>
        /// <param name="healAmount">治疗量</param>
        public void HealPlayer(int healAmount)
        {
            float previousHealth = PlayerCurrentHealth;
            PlayerCurrentHealth = Mathf.Min(PlayerMaxHealth, PlayerCurrentHealth + healAmount);
            
            // 立即更新UI显示
            UpdateHealthDisplay();
            
            Debug.Log($"玩家恢复 {healAmount} 点血量，血量: {PlayerCurrentHealth}/{PlayerMaxHealth}");
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
