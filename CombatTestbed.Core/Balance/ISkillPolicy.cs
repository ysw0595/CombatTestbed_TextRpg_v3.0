#nullable enable
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 전투에서 어떤 스킬을 쓸지 정하는 전략입니다. 사람 플레이어의 "플레이 방식"을 흉내 냅니다.
    /// </summary>
    public interface ISkillPolicy
    {
        string Name { get; }

        /// <summary>
        /// 지금 배운 스킬 중 하나를 골라 돌려줍니다. 반드시 배운 스킬이어야 합니다.
        /// </summary>
        SkillDefinition Choose(CampaignManager campaign, IRandomSource random);
    }
}