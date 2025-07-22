using Module.Battle;

namespace Module.Interfaces.Attributes
{
    /// <summary>
    /// 攻击属性接口，定义攻击属性的行为
    /// </summary>
    public interface IAttackAttribute
    {
        void ApplyAttribute(AttackContext context);
    }
} 