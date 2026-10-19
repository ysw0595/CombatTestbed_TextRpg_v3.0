using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 효과 하나를 실행한 결과입니다. (어떤 종류의 효과가, 누구에게, 실제로 얼마나 적용되었는가)
    /// </summary>
    public readonly struct EffectOutcome
    {
        public SkillEffectKind Kind { get; }
        public BattleActor Receiver { get; }

        /// <summary>
        /// 실제로 적용된 양. 피해는 실피해, 회복은 실회복량, 상태이상은 스택 수, 스탯 증가는 증가량입니다.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// 스탯 증가(StatGain)일 때 어떤 스탯이 올랐는지. 다른 종류에서는 null입니다.
        /// </summary>
        public StatType? Stat { get; }

        public EffectOutcome(SkillEffectKind kind, BattleActor receiver, int amount, StatType? stat = null)
        {
            Kind = kind;
            Receiver = receiver;
            Amount = amount;
            Stat = stat;
        }
    }
}