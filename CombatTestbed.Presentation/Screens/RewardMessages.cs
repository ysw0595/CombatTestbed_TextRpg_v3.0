#nullable enable
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 보상 결과(EffectOutcome)를 화면에 보여 줄 문구로 바꿉니다.
    /// 보여 줄 내용이 없는 결과(회복량 0 등)는 null을 돌려줍니다.
    /// </summary>
    public static class RewardMessages
    {
        public static string? Format(EffectOutcome outcome)
        {
            switch (outcome.Kind)
            {
                case SkillEffectKind.StatGain:
                    return outcome.Amount > 0
                        ? $"{StatSubject(outcome.Stat)} {outcome.Amount} 증가했다."
                        : null;

                case SkillEffectKind.Heal:
                    return outcome.Amount > 0
                        ? $"체력을 {outcome.Amount} 회복했다."
                        : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// 능력치 이름에 알맞은 조사(이/가)를 붙인 주어를 돌려줍니다.
        /// (받침 유무를 계산하는 대신, 능력치마다 알맞은 형태를 직접 지정)
        /// </summary>
        private static string StatSubject(StatType? stat)
        {
            if (!stat.HasValue) return "능력치가";

            switch (stat.Value)
            {
                case StatType.MaxHp:
                    return "최대 체력이";
                case StatType.Attack:
                    return "공격력이";
                case StatType.Defense:
                    return "방어력이";
                default:
                    return "능력치가";
            }
        }
    }
}