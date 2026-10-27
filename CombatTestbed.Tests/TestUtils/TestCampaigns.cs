using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 화면 Presenter 테스트에서 쓰는 작은 캠페인을 만들어 주는 도우미입니다.
    /// 챕터 이름은 "c1"이라 화면 제목은 "챕터 1 : c1 (stage 1)"이 됩니다.
    /// </summary>
    public static class TestCampaigns
    {
        // 용사(공격력 50)가 쓰는 기본 스킬: 50 피해
        public static readonly SkillDefinition Strike = new SkillDefinition("strike", "strike", new[]
        {
            SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
        });

        public static ActorDefinition Hero(int maxHp = 100, int attack = 50)
        {
            return new ActorDefinition("hero", "용사", ActorRole.Player, maxHp, attack);
        }

        public static ActorDefinition Monster(string id, int maxHp, int attack, RewardDefinition reward = null)
        {
            return new ActorDefinition(id, id, ActorRole.Enemy, maxHp, attack, reward: reward);
        }

        /// <summary>
        /// 단계 하나짜리 챕터로 만든 캠페인 (시작 스킬은 Strike)
        /// </summary>
        public static CampaignManager SingleStage(
            StageDefinition stage, ActorDefinition monster = null, SkillDefinition[] skillPool = null)
        {
            var pool = monster == null ? null : new[] { monster };
            var chapter = new ChapterDefinition("c1", "c1", new[] { stage }, pool);

            return new CampaignManager(
                new[] { chapter }, Hero(), new[] { Strike },
                skillPool ?? new SkillDefinition[0], new SequenceRandomSource(0, 0, 0, 0));
        }
    }
}