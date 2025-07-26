using System.Collections.Generic;
using Enemy;
using Module.Battle;
using UnityEngine;

namespace Module
{
    public abstract class BaseAttackModule : BaseModule
    {
        [SerializeField] protected GameObject _bulletPrefab; // 子弹预制体
        [SerializeField] protected Transform _firePoint;
        public AttackParameters AttackParameters { get; set; }
        public bool CanAttack()
        {
            return AttackParameters.canAttack;
        }
        
        public void StartAttackCD()
        {
            AttackParameters.canAttack = false;
            AttackParameters.attackCD = 1f / AttackParameters.attackSpeed;
        }
        
        public AttackParameters GetAttackParameters()
        {
            return AttackParameters.Clone();
        }

        public void ExecuteAttack(GameObject target)
        {
            if (!_bulletPrefab || !target)
            {
                Debug.LogWarning("子弹预制体或目标为空，无法发射子弹");
                return;
            }
            
            // 创建攻击上下文
            AttackContext context = new AttackContext(this, target, GetAttackParameters());
            
            // 调用子弹管理器生成子弹
            StartCoroutine(Controllers.Battle.BulletManager.Instance.SpawnBullets(context));
        }
        
         public List<GameObject> GetTargetsInRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取模块在网格中的位置信息
            Controllers.ModulesManager.ModuleInfo moduleInfo = Controllers.ModulesManager.Instance.GetModuleInfoByModule(this);
            
            Vector3Int gridOffset = moduleInfo.gridOffset;
            
            // 根据模块在网格中的位置确定攻击范围类型
            if (gridOffset.y > 0)
            {
                // 上方模块 - 圆环柱形攻击范围
                targets = GetTargetsInRingRange();
            }
            else if (gridOffset.y < 0)
            {
                // 下方模块 - 圆柱形攻击范围
                targets = GetTargetsInCylinderRange();
            }
            else if (gridOffset.x != 0 || gridOffset.z != 0)
            {
                // 前后左右模块 - 扇形柱形攻击范围
                targets = GetTargetsInSectorRange(gridOffset);
            }
            
            return targets;
        }
         
        // 扇形柱形攻击范围（用于前后左右模块）
        private List<GameObject> GetTargetsInSectorRange(Vector3Int gridOffset)
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 确定攻击方向
            Vector3 attackDirection = Vector3.zero;
            if (gridOffset.x > 0) attackDirection = Vector3.right;      // 右侧模块向右攻击
            else if (gridOffset.x < 0) attackDirection = Vector3.left;  // 左侧模块向左攻击
            else if (gridOffset.z > 0) attackDirection = Vector3.forward; // 前方模块向前攻击
            else if (gridOffset.z < 0) attackDirection = Vector3.back;   // 后方模块向后攻击
            
            // 获取中心点位置（核心立方体位置）
            Vector3 centerPosition = Controllers.ModulesManager.Instance.GetCenterModule().transform.position;
            
            // 使用更大范围的球体检测，确保能覆盖到扇形柱体的所有可能区域
            // 对于90度扇形，使用1.5倍半径的检测球可以确保覆盖所有可能区域
            Collider[] colliders = Physics.OverlapSphere(centerPosition, AttackParameters.attackRange * 1.5f, 1 << 8);
            
            foreach (var enemyCollider in colliders)
            {
                if (!enemyCollider.TryGetComponent<BaseEnemy>(out _)) continue;
                
                Vector3 enemyPosition = enemyCollider.transform.position;
                
                // 计算敌人在水平面上的投影点到中心的向量
                Vector3 horizontalDirToEnemy = new Vector3(
                    enemyPosition.x - centerPosition.x, 
                    0, 
                    enemyPosition.z - centerPosition.z
                ).normalized;
                
                // 检查敌人是否在扇形范围内（90度圆心角）
                float angle = Vector3.Angle(attackDirection, horizontalDirToEnemy);
                
                // 计算敌人到中心点的水平距离
                float horizontalDistance = Vector2.Distance(
                    new Vector2(enemyPosition.x, enemyPosition.z),
                    new Vector2(centerPosition.x, centerPosition.z)
                );
                
                // 检查敌人是否在扇形范围内：角度小于45度且距离小于攻击范围
                if (angle <= 45f && horizontalDistance <= AttackParameters.attackRange)
                {
                    targets.Add(enemyCollider.gameObject);
                    Debug.Log("扇形检测到敌人：" + enemyCollider.name);
                }
            }
            
            return targets;
        }
        
        // 圆环柱形攻击范围（用于上方模块）
        private List<GameObject> GetTargetsInRingRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取中心点位置（核心立方体位置）
            Vector3 centerPosition = Controllers.ModulesManager.Instance.GetCenterModule().transform.position;
            
            // 使用更大范围的球体检测，确保能覆盖到圆环柱体的所有可能区域
            Collider[] colliders = Physics.OverlapSphere(centerPosition, AttackParameters.attackRange * 1.5f, 1 << 8);
            
            foreach (var enemyCollider in colliders)
            {
                if (!enemyCollider.TryGetComponent<BaseEnemy>(out _)) continue;
                
                Vector3 enemyPosition = enemyCollider.transform.position;
                
                // 计算敌人到中心点的水平距离（忽略y轴）
                float horizontalDistance = Vector2.Distance(
                    new Vector2(enemyPosition.x, enemyPosition.z),
                    new Vector2(centerPosition.x, centerPosition.z)
                );
                
                // 检查敌人是否在圆环范围内（外半径为_attackRange，内半径为_attackRange/2）
                if (horizontalDistance >= AttackParameters.attackRange / 2 && horizontalDistance <= AttackParameters.attackRange)
                {
                    targets.Add(enemyCollider.gameObject);
                    Debug.Log("圆环检测到敌人：" + enemyCollider.name);
                }
            }
            
            return targets;
        }
        
        // 圆柱形攻击范围（用于下方模块）
        private List<GameObject> GetTargetsInCylinderRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取中心点位置（核心立方体位置）
            Vector3 centerPosition = Controllers.ModulesManager.Instance.GetCenterModule().transform.position;
            
            // 使用更大范围的球体检测，确保能覆盖到圆柱体的所有可能区域
            Collider[] colliders = Physics.OverlapSphere(centerPosition, AttackParameters.attackRange * 1.5f, 1 << 8);
            
            foreach (var enemyCollider in colliders)
            {
                if (!enemyCollider.TryGetComponent<BaseEnemy>(out _)) continue;
                
                Vector3 enemyPosition = enemyCollider.transform.position;
                
                // 计算敌人到中心点的水平距离（忽略y轴）
                float horizontalDistance = Vector2.Distance(
                    new Vector2(enemyPosition.x, enemyPosition.z),
                    new Vector2(centerPosition.x, centerPosition.z)
                );
                
                // 检查敌人是否在圆柱范围内（以_attackRange/2为半径）
                if (horizontalDistance <= AttackParameters.attackRange/2)
                {
                    targets.Add(enemyCollider.gameObject);
                    Debug.Log("圆形检测到敌人：" + enemyCollider.name);
                }
            }
            
            return targets;
        }
    }
}
