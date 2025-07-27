using System;
using System.Runtime.Serialization;
using Enemy.Interfaces;
using UnityEngine;
using Enemy.AttackStrategies;
using Enemy.MovementStrategies;
using Controllers;
using Enemy.Enums;
using AttackType = Enemy.Enums.AttackType;
using BattleManager = Controllers.Battle.BattleManager;


namespace Enemy
{
    /// <summary> 基础敌人类，所有敌人类都应继承自此类 </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Enemy/BaseEnemy")]
    [Serializable]
    public abstract class BaseEnemy : MonoBehaviour
    {
        [Header("基础敌人属性")]
        /// <summary> 血量 </summary>
        [Header("血量")]public int health = 100;
        /// <summary> 移动速度 </summary>
        [Header("移动速度")]public float moveSpeed = 5f;
        /// <summary> 攻击力 </summary>
        [Header("攻击力")]public int attackPower = 10;
        
        
        [Header("移动配置")]
        /// <summary>移动策略类型</summary>
        [Header("移动策略")]public MovementType movementType;
        /// <summary>环绕半径（环绕移动时使用）</summary>
        [Header("环绕半径")]public float orbitRadius = 5f;

        
        [Header("攻击配置")]
        /// <summary>攻击策略类型</summary>
        [Header("攻击策略")]public AttackType attackType;
        /// <summary>攻击范围</summary>
        [Header("攻击范围")]public float attackRange = 2f;
        /// <summary>攻击冷却时间</summary>
        [Header("攻击冷却")]public float attackCooldown = 1f;
        /// <summary>投射物预制体（远程攻击时使用）</summary>
        [Header("子弹预制体")] public GameObject projectilePrefab;

        protected Rigidbody Rb;
        protected Collider Col;
        protected IMovable MovementStrategy;
        protected IAttackable AttackStrategy;
        
        /// <summary>上次攻击的时间</summary>
        private float _lastAttackTime;
        
        /// <summary> 初始化组件 </summary>
        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Col = GetComponent<Collider>();
        }

        /// <summary> 启动时调用，自动注册到BattleManager </summary>
        protected virtual void Start()
        {
            // 注册到BattleManager
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RegisterEnemy(this);
            }
        }

        /// <summary> 由BattleManager统一调用的逻辑更新方法 </summary>
        public virtual void OnBattleUpdate()
        {
            // 检查目标是否丢失，如果丢失则重新寻找
            if (MovementStrategy != null && MovementStrategy.GetTarget() == null)
            {
                FindAndSetTarget();
            }

            ExecuteMovement();
            ExecuteAttack();
        }

        /// <summary> 执行移动逻辑 </summary>
        protected virtual void ExecuteMovement()
        {
            if (MovementStrategy != null)
            {
                MovementStrategy.Move(transform, Rb, moveSpeed);
            }
        }

        /// <summary> 执行攻击逻辑 </summary>
        protected virtual void ExecuteAttack()
        {
            if (AttackStrategy != null)
            {
                Transform target = MovementStrategy?.GetTarget();
                if (target != null && Time.time - _lastAttackTime >= attackCooldown && AttackStrategy.Attack(transform, target, attackPower))
                {
                    _lastAttackTime = Time.time; // 更新最后攻击时间
                    OnAttackExecuted();
                }
            }
        }

        /// <summary> 攻击执行后的回调</summary>
        protected virtual void OnAttackExecuted()
        {
            
        }

        /// <summary> 设置移动策略 </summary>
        /// <param name="strategy">移动策略</param>
        public virtual void SetMovementStrategy(IMovable strategy)
        {
            MovementStrategy = strategy;
        }

        /// <summary> 设置攻击策略 </summary>
        /// <param name="strategy">攻击策略</param>
        public virtual void SetAttackStrategy(IAttackable strategy)
        {
            AttackStrategy = strategy;
        }

        /// <summary> 获取当前移动策略 </summary>
        /// <returns>当前移动策略</returns>
        public virtual IMovable GetMovementStrategy() => MovementStrategy;

        /// <summary> 获取当前攻击策略 </summary>
        /// <returns>当前攻击策略</returns>
        public virtual IAttackable GetAttackStrategy() => AttackStrategy;

        /// <summary> 设置目标 </summary>
        /// <param name="target">目标Transform</param>
        public virtual void SetTarget(Transform target)
        {
            if (MovementStrategy != null)
            {
                MovementStrategy.SetTarget(target);
            }

        }

        /// <summary> 受到伤害 </summary>
        /// <param name="damage">伤害值</param>
        public virtual void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
            Debug.Log($"{name}受到伤害: {damage}, 剩余血量: {health}");
        }

        /// <summary> 死亡方法 </summary>
        protected virtual void Die()
        {
            // 从BattleManager注销
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.UnregisterEnemy(this);
            }
            Destroy(gameObject);
        }

        /// <summary> 当对象被销毁时，确保从BattleManager注销 </summary>
        protected virtual void OnDestroy()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.UnregisterEnemy(this);
            }
        }
        
        /// <summary>初始化移动策略</summary>
        protected virtual void InitializeMovementStrategy()
        {
            switch (movementType)
            {
                case MovementType.Straight:
                    StraightMovement straightMovement = new StraightMovement();
                    straightMovement.SetAttackRange(attackRange); // 设置攻击范围
                    SetMovementStrategy(straightMovement);
                    break;
            }
        }
        
        /// <summary>初始化攻击策略</summary>
        protected virtual void InitializeAttackStrategy()
        {
            switch (attackType)
            {              
                case AttackType.Melee:
                    MeleeAttack meleeAttack = new MeleeAttack();
                    meleeAttack.SetAttackParameters(attackRange, attackCooldown);
                    SetAttackStrategy(meleeAttack);
                    break;
                
                case AttackType.None:
                    break;  
                    
                case AttackType.Ranged:
                    RangedAttack rangedAttack = new RangedAttack();
                    SetAttackStrategy(rangedAttack);
                    break;
            }
        }
        
        /// <summary>寻找并设置中心模块为目标</summary>
        protected virtual void FindAndSetTarget()
        {
            if (ModulesManager.Instance != null)
            {
                Module.BaseModule centerModule = ModulesManager.Instance.GetCenterModule();
                if (centerModule != null)
                {
                    SetTarget(centerModule.transform);
                }
            }
        }

    }
}
