using Module.Enums;
using Module.Interfaces;

namespace Module.Battle
{
    /// <summary>
    /// 攻击属性工厂，根据攻击属性枚举用于创建攻击属性实现
    /// </summary>
    public static class AttackAttributeFactory
    {
        public static IAttackAttribute CreateAttribute(AttackAttribute attribute)
        {
            switch (attribute)
            {
                case AttackAttribute.None:
                    return new NormalAttribute();
                
                case AttackAttribute.Splash:
                    return new SplashAttribute();
                
                case AttackAttribute.Continuous:
                    // TODO: 实现持续性伤害属性
                    return new NormalAttribute();
                
                default:
                    return new NormalAttribute();
            }
        }
    }
} 