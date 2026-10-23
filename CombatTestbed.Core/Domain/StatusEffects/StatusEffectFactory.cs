using System;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 상태이상 종류와 스택 수로부터 효과 인스턴스를 만드는 팩토리입니다.
    /// 스킬 데이터(종류 + 스택)를 실제 효과 객체로 바꾸는 연결점입니다.
    /// </summary>
    public static class StatusEffectFactory
    {
        public static IStatusEffect Create(StatusEffectType type, int stacks)
        {
            switch (type)
            {
                case StatusEffectType.Poison:
                    return new PoisonEffect(stacks);
                case StatusEffectType.Bleed:
                    return new BleedEffect(stacks);
                case StatusEffectType.Paralysis:
                    return new ParalysisEffect(stacks);
                case StatusEffectType.BloodDiamond:
                    return new BloodDiamondEffect(stacks);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type), type, "지원하지 않는 상태 이상 종류입니다.");
            }
        }
    }
}