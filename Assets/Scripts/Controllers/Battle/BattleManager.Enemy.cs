using System.Collections.Generic;
using Module;
using UnityEngine;

namespace Controllers.Battle
{
    public partial class BattleManager
    {
        public GameObject _defaultEnemyPrefab;
        
        public float _spawnHeightOffset = 0.5f;
        
        private List<Enemy.BaseEnemy> _registeredEnemies = new List<Enemy.BaseEnemy>();
        
        private const int _groundLayer = 3;
        private const int _enemyLayer = 8;
        
        public void RegisterEnemy(Enemy.BaseEnemy enemy)
        {
            if (enemy != null && !_registeredEnemies.Contains(enemy))
            {
                _registeredEnemies.Add(enemy);
                Debug.Log($"敌人 {enemy.name} 已注册到战斗管理器");
            }
        }
        
        public void UnregisterEnemy(Enemy.BaseEnemy enemy)
        {
            if (_registeredEnemies.Contains(enemy))
            {
                _registeredEnemies.Remove(enemy);
                Debug.Log($"敌人 {enemy.name} 已从战斗管理器注销");
            }
        }
        
        public int GetEnemyCount()
        {
            return _registeredEnemies.Count;
        }
        
        public List<Enemy.BaseEnemy> GetAllEnemies()
        {
            return new List<Enemy.BaseEnemy>(_registeredEnemies);
        }
        
        public void ClearAllEnemies()
        {
            foreach (Enemy.BaseEnemy enemy in _registeredEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            _registeredEnemies.Clear();
        }
        
        private void GenerateEnemy(GameObject enemyPrefab = null)
        {
            // 检查是否传入了敌人预制体
            if (enemyPrefab is not null)
            {
                _defaultEnemyPrefab = enemyPrefab;
            }

            // 获取摄像机实例
            Camera cam = CameraScript.Instance.camera;
    
            // 创建从摄像机到鼠标位置的射线
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            
            // 进行射线投射，只检测指定的地面层级
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << _groundLayer))
            {
                // 在击中点上方生成敌人，避免生成在地面下
                Vector3 spawnPosition = hit.point + Vector3.up * _spawnHeightOffset;
                GameObject enemyObj = Instantiate(_defaultEnemyPrefab, spawnPosition, Quaternion.identity);

                // 确保敌人正确朝向中心模块
                if (ModulesManager.Instance?.GetCenterModule() != null)
                {
                    enemyObj.transform.LookAt(ModulesManager.Instance.GetCenterModule().transform);
                }

                enemyObj.layer = _enemyLayer;
                
                Debug.Log($"在Ground层的位置 {hit.point} 生成了敌人");
            }
            else
            {
                Debug.Log("射线未击中Ground层");
            }
        }
        
        public void DamagePlayer(int damage)
        {
            float previousHealth = PlayerCurrentHealth;
            PlayerCurrentHealth = Mathf.Max(0, PlayerCurrentHealth - damage);
            
            // 敌人攻击时立即更新血量显示
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateHealthText(PlayerCurrentHealth, PlayerMaxHealth);
            }
            
            Debug.Log($"玩家受到 {damage} 点伤害，血量: {PlayerCurrentHealth}/{PlayerMaxHealth}");
            
            if (PlayerCurrentHealth <= 0 && previousHealth > 0)
            {
                OnPlayerDeath();
            }
        }

        private void UpdateEnemyLogic()
        {
            // 倒序遍历，防止在遍历过程中移除敌人时出现索引错误
            for (int i = _registeredEnemies.Count - 1; i >= 0; i--)
            {
                if (_registeredEnemies[i] == null)
                {
                    // 如果敌人被销毁，从列表中移除
                    _registeredEnemies.RemoveAt(i);
                }
                else
                {
                    // 调用敌人的逻辑更新
                    _registeredEnemies[i].OnBattleUpdate();
                }
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                GenerateEnemy();
            }
        }
    }
}