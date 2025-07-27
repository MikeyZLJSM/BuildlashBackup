using System;
using Module.Battle;
using Module.Enums;
using Module.Interfaces;
using UnityEngine;

namespace Controllers.Battle
{
    /// <summary>
    /// 子弹基类，实现基本的飞行、追踪和伤害逻辑
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("碰撞设置")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private LayerMask _targetLayerMask;
        
        // 攻击上下文
        private AttackContext _context;
        
        // 子弹属性
        private Transform _target;
        private float _speed;
        private float _lifetime;
        private float _spawnTime;
        
        // 攻击属性实现
        private IAttackAttribute _attackAttribute;
        
        // 状态
        private bool _isInitialized = false;
        private bool _isHoming = true; // 是否追踪目标
        
        // 回调
        public Action OnDeactivate { get; set; }
        
        /// <summary>
        /// 初始化子弹属性
        /// </summary>
        public void Initialize(AttackContext context)
        {
            _context = context;
            _target = context.target?.transform;
            _speed = context.parameters.bulletSpeed;
            _lifetime = 10f; // 默认生命周期
            _spawnTime = Time.time;
            _isInitialized = true;
            _isHoming = true;
            
            // 创建攻击属性实现
            _attackAttribute = AttackAttributeFactory.CreateAttribute(context.parameters.attackAttribute);
            
            // 设置子弹朝向目标
            if (_target)
            {
                transform.LookAt(_target);
            }
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            
            // 检查生命周期
            if (IsExpired())
            {
                Deactivate();
                return;
            }
            
            // 检查目标是否存在
            if (!_target)
            {
                // 目标不存在时，继续沿当前方向飞行
                _isHoming = false;
            }
            
            // 移动子弹
            MoveBullet();
            
            // 检测碰撞
            CheckCollision();
        }
        
        private void MoveBullet()
        {
            if (_isHoming && _target != null)
            {
                // 追踪目标
                Vector3 direction = (_target.position - transform.position).normalized;
                transform.forward = direction;
                transform.position += direction * (_speed * Time.deltaTime);
            }
            else
            {
                // 沿当前方向飞行
                transform.position += transform.forward * (_speed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// 检测碰撞
        /// </summary>
        private void CheckCollision()
        {
            // 使用OverlapSphere检测碰撞
            Collider[] colliders = Physics.OverlapSphere(transform.position, _collisionRadius, _targetLayerMask);
            
            foreach (var collider in colliders)
            {
                // 检查是否击中敌人
                if (collider.gameObject == _context.target)
                {
                    // 更新击中点
                    _context.SetImpactPoint(transform.position);
                    
                    // TODO: 通知应用攻击属性
                    _attackAttribute.ApplyAttribute(_context);
                    
                    // 子弹命中后失活
                    Deactivate();
                    return;
                }
            }
        }
        
        /// <summary>
        /// 检查子弹是否过期
        /// </summary>
        public bool IsExpired()
        {
            return Time.time - _spawnTime >= _lifetime;
        }
        
        /// <summary>
        /// 使子弹失活
        /// </summary>
        public void Deactivate()
        {
            _isInitialized = false;
            OnDeactivate?.Invoke();
        }
        
        private void OnDrawGizmosSelected()
        {
            // 绘制碰撞检测范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _collisionRadius);
        }
    }
} 