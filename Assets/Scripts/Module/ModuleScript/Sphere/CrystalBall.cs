using System.Collections.Generic;
using Enemy;
using Module.Battle;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Module.ModuleScript.Sphere
{
    public class CrystalBall : NormalSphere, IAttackable
    {
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private GameObject _bulletPrefab; // 子弹预制体
        [SerializeField] private Transform _firePoint; // 发射点
        
        private AttackParameters _modifiedAttackParameters;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化攻击参数
            _attackParameters = new AttackParameters
            {
                targetLockType = TargetLockType.Nearest,
                bulletCount = 2,
                targetCount = 2,
                damageType = DamageType.Magical,
                attackAttribute = AttackAttribute.None,
                damage = 8,
                attackSpeed = 1.0f,
                attackRange = _attackRange,
                bulletSpeed = 10f,
                bulletPrefab = _bulletPrefab
            };
            
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }
        
        
        public bool CanAttack()
        {
            return _canAttack;
        }

        public void StartAttackCD()
        {
            _canAttack = false;
            _attackCD = 1f / _attackParameters.attackSpeed;
        }

        public List<GameObject> GetTargetsInRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取模块在网格中的位置信息
            Controllers.ModulesManager.ModuleInfo moduleInfo = Controllers.ModulesManager.Instance.GetModuleInfoByModule(this);
            if (moduleInfo == null)
            {
                Debug.LogWarning("无法找到模块信息，使用默认攻击范围");
                return GetTargetsInDefaultRange();
            }
            
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
            else
            {
                // 中心模块 - 使用默认球形范围
                targets = GetTargetsInDefaultRange();
            }
            
            return targets;
        }
        
        // 默认球形范围（用于中心模块或无法确定位置的模块）
        private List<GameObject> GetTargetsInDefaultRange()
        {
            List<GameObject> targets = new List<GameObject>();
            
            // 获取攻击范围内的所有敌人
            Collider[] colliders = Physics.OverlapSphere(
                transform.position, 
                _attackRange, 
                1 << 8 // 敌人图层
            );
            
            // 添加找到的敌人到目标列表
            foreach (var enemyCollider in colliders)
            {
                if (enemyCollider.TryGetComponent<BaseEnemy>(out _))
                {
                    targets.Add(enemyCollider.gameObject);
                }
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
            Collider[] colliders = Physics.OverlapSphere(centerPosition, _attackRange * 1.5f, 1 << 8);
            
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
                if (angle <= 45f && horizontalDistance <= _attackRange)
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
            Collider[] colliders = Physics.OverlapSphere(centerPosition, _attackRange * 1.5f, 1 << 8);
            
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
                if (horizontalDistance >= _attackRange / 2 && horizontalDistance <= _attackRange)
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
            Collider[] colliders = Physics.OverlapSphere(centerPosition, _attackRange * 1.5f, 1 << 8);
            
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
                if (horizontalDistance <= _attackRange/2)
                {
                    targets.Add(enemyCollider.gameObject);
                    Debug.Log("圆形检测到敌人：" + enemyCollider.name);
                }
            }
            
            return targets;
        }
        
        public AttackParameters GetAttackParameters()
        {
            // 返回攻击参数的副本，以防被修改
            return _attackParameters.Clone();
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
        
        private void OnDrawGizmosSelected()
        {
            // 获取模块在网格中的位置信息
            Controllers.ModulesManager.ModuleInfo moduleInfo = null;
            Vector3 centerPosition = Vector3.zero;
            
            // 尝试获取模块信息和中心位置
            if (Controllers.ModulesManager.Instance != null)
            {
                moduleInfo = Controllers.ModulesManager.Instance.GetModuleInfoByModule(this);
                var centerModule = Controllers.ModulesManager.Instance.GetCenterModule();
                if (centerModule != null)
                {
                    centerPosition = centerModule.transform.position;
                }
            }
            
            // 如果无法获取模块信息，使用默认可视化
            if (moduleInfo == null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _attackRange);
                return;
            }
            
            Vector3Int gridOffset = moduleInfo.gridOffset;
            
            // 根据模块在网格中的位置绘制不同的攻击范围
            if (gridOffset.y > 0)
            {
                // 上方模块 - 圆环柱形攻击范围
                DrawRingRangeGizmos(centerPosition);
            }
            else if (gridOffset.y < 0)
            {
                // 下方模块 - 圆柱形攻击范围
                DrawCylinderRangeGizmos(centerPosition);
            }
            else if (gridOffset.x != 0 || gridOffset.z != 0)
            {
                // 前后左右模块 - 扇形柱形攻击范围
                DrawSectorRangeGizmos(gridOffset, centerPosition);
            }
            else
            {
                // 中心模块 - 使用默认球形范围
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _attackRange);
            }
        }
        
        // 绘制扇形柱形攻击范围
        private void DrawSectorRangeGizmos(Vector3Int gridOffset, Vector3 centerPosition)
        {
            // 确定攻击方向
            Vector3 attackDirection = Vector3.zero;
            if (gridOffset.x > 0) attackDirection = Vector3.right;      // 右侧模块向右攻击
            else if (gridOffset.x < 0) attackDirection = Vector3.left;  // 左侧模块向左攻击
            else if (gridOffset.z > 0) attackDirection = Vector3.forward; // 前方模块向前攻击
            else if (gridOffset.z < 0) attackDirection = Vector3.back;   // 后方模块向后攻击
            
            // 设置Gizmos颜色
            Gizmos.color = new Color(1, 0, 0, 0.3f); // 半透明红色
            
            // 绘制扇形的边界线
            Vector3[] directions = new Vector3[3];
            directions[0] = Quaternion.Euler(0, -45, 0) * attackDirection; // 左边界
            directions[1] = attackDirection; // 中心方向
            directions[2] = Quaternion.Euler(0, 45, 0) * attackDirection; // 右边界
            
            // 绘制扇形边界线
            for (int i = 0; i < directions.Length; i++)
            {
                Gizmos.DrawLine(centerPosition, centerPosition + directions[i] * _attackRange);
            }
            
            // 绘制扇形弧线
            DrawArcGizmos(centerPosition, directions[0] * _attackRange, directions[2] * _attackRange, 10);
            
            // 绘制检测范围的外边界（用于调试）
            Gizmos.color = new Color(0, 1, 1, 0.1f); // 半透明青色
            Gizmos.DrawWireSphere(centerPosition, _attackRange * 1.5f);
        }
        
        // 绘制圆环柱形攻击范围
        private void DrawRingRangeGizmos(Vector3 centerPosition)
        {
            // 绘制外圆
            Gizmos.color = new Color(1, 0, 0, 0.3f); // 半透明红色
            DrawCircleGizmos(centerPosition, _attackRange, 30);
            
            // 绘制内圆
            Gizmos.color = new Color(1, 1, 0, 0.3f); // 半透明黄色
            DrawCircleGizmos(centerPosition, _attackRange / 2, 20);
        }
        
        // 绘制圆柱形攻击范围
        private void DrawCylinderRangeGizmos(Vector3 centerPosition)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // 半透明红色
            
            // 绘制顶部和底部圆
            DrawCircleGizmos(new Vector3(centerPosition.x, centerPosition.y, centerPosition.z), _attackRange, 30);
        }
        
        // 绘制圆形Gizmos辅助方法
        private void DrawCircleGizmos(Vector3 center, float radius, int segments)
        {
            Vector3[] points = new Vector3[segments + 1];
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2;
                points[i] = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                ) + center;
            }
            
            for (int i = 0; i < segments; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }
        
        // 绘制弧线Gizmos辅助方法
        private void DrawArcGizmos(Vector3 center, Vector3 from, Vector3 to, int segments)
        {
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 point = Vector3.Slerp(from, to, t);
                Vector3 nextPoint = Vector3.Slerp(from, to, (float)(i + 1) / segments);
                
                Gizmos.DrawLine(center + point, center + nextPoint);
            }
        }
    }
}