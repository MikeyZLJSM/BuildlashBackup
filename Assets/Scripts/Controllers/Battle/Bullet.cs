using System;
using Module.Enums;
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
        
        // 子弹属性
        private Transform _target;
        private int _damage;
        private DamageType _damageType;
        private float _speed;
        private float _lifetime;
        private float _spawnTime;
        
        // 状态
        private bool _isInitialized = false;
        private bool _isHoming = true; // 是否追踪目标
        
        // 回调
        public Action OnDeactivate { get; set; }
        
        /// <summary>
        /// 初始化子弹属性
        /// </summary>
        public void Initialize(Transform target, int damage, DamageType damageType, float speed, float lifetime)
        {
            _target = target;
            _damage = damage;
            _damageType = damageType;
            _speed = speed;
            _lifetime = lifetime;
            _spawnTime = Time.time;
            _isInitialized = true;
            _isHoming = true;
            
            // 设置子弹朝向目标
            if (_target != null)
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
            if (_target == null)
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
        
        private void CheckCollision()
        {
            //TODO：是否可以用自身碰撞体检测与敌人的碰撞
            Collider[] colliders = Physics.OverlapSphere(transform.position, _collisionRadius, _targetLayerMask);
            
            foreach (var enemyCollider in colliders)
            {
                if (enemyCollider.TryGetComponent<Enemy.BaseEnemy>(out var enemy))
                {
                    enemy.TakeDamage(_damage);
                    
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
        /// 子彈失效
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