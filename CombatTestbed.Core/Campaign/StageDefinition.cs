#nullable enable
using System;
using CombatTestbed.Core.Rewards;

namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 챕터 안의 단계 하나의 정의입니다. (순수 데이터)
    /// 스토리 문구는 데이터에 넣지 않고, 화면(View)이 챕터/단계 번호로 찾아서 보여 줍니다.
    /// </summary>
    public sealed class StageDefinition
    {
        public StageType Type { get; }

        /// <summary>
        /// 이 단계를 마치면 플레이어가 무작위 스킬 하나를 배웁니다.
        /// </summary>
        public bool GrantsSkill { get; }

        /// <summary>
        /// 이 단계를 마치면 플레이어에게 적용되는 보상(강화, 체력 회복 등). 없으면 null입니다.
        /// </summary>
        public RewardDefinition? ClearReward { get; }

        public StageDefinition(StageType type, bool grantsSkill = false, RewardDefinition? clearReward = null)
        {
            bool canHaveClearEffects = type == StageType.Episode || type == StageType.Boss;

            if (!canHaveClearEffects && (grantsSkill || clearReward != null))
            {
                throw new ArgumentException("스킬 획득과 클리어 보상은 에피소드/보스 단계에만 둘 수 있습니다.");
            }

            Type = type;
            GrantsSkill = grantsSkill;
            ClearReward = clearReward;
        }
    }
}