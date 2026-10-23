#nullable enable
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Definitions;

namespace CombatTestbed.Core.Content
{
    /// <summary>
    /// 원본 게임 한 판(흡혈귀로 3개 챕터)을 조립해 주는 팩토리입니다.
    /// </summary>
    public static class DefaultCampaign
    {
        /// <param name="random">난수 공급자 (몬스터 순환, 스킬 습득, 보스 패턴 시작에 사용)</param>
        /// <param name="player">플레이어 정의. 생략하면 기본 흡혈귀입니다. (밸런스 테스트에 사용)</param>
        public static CampaignManager Create(IRandomSource random, ActorDefinition? player = null)
        {
            return new CampaignManager(
                DefaultChapters.All,
                player ?? DefaultActors.Vampire,
                new[] { DefaultSkills.BatFire },
                DefaultSkills.VampireSkills,
                random);
        }
    }
}