using Controllers;
using Enemy.MovementStrategies;
using Enemy.AttackStrategies;
using Enemy.Enums;
using Enemy.Interfaces;
using AttackType = Enemy.Enums.AttackType;


namespace Enemy.EnemyScript
{
    /// <summary>简单的敌人类，继承自BaseEnemy</summary>
    
    public class SimpleEnemy : BaseEnemy
    {
        /// <summary>初始化</summary>
        protected override void Start()
        {
            // 设置移动和攻击策略类型
            movementType = MovementType.Straight;
            attackType = AttackType.Melee;
            
            base.Start();
            
            InitializeMovementStrategy();
            InitializeAttackStrategy();
            FindAndSetTarget();
        }
        
        /// <summary>初始化移动策略 - 配置直线移动</summary>
        protected override void InitializeMovementStrategy()
        {
            StraightMovement straightMovement = new StraightMovement();
            SetMovementStrategy(straightMovement);
        }
        
        /// <summary>初始化攻击策略 - 配置近战攻击</summary>
        protected override void InitializeAttackStrategy()
        {
            MeleeAttack meleeAttack = new MeleeAttack();
            meleeAttack.SetAttackParameters(attackRange, attackCooldown);
            SetAttackStrategy(meleeAttack);
        }

    }
}
