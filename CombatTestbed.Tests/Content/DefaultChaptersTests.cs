using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Content;

namespace CombatTestbed.Tests.Content
{
    [TestFixture]
    public class DefaultChaptersTests
    {
        [Test]
        public void 챕터는_3개이고_모두_원본과_같은_10단계_구성이어야_한다()
        {
            var expectedLayout = new[]
            {
                StageType.Prologue,
                StageType.Battle, StageType.Battle, StageType.Battle, StageType.Battle,
                StageType.Episode,
                StageType.Battle, StageType.Battle, StageType.Battle,
                StageType.Boss
            };

            Assert.That(DefaultChapters.All.Count, Is.EqualTo(3));
            Assert.That(DefaultChapters.All.Select(c => c.Name),
                Is.EqualTo(new[] { "폭풍전야", "붉은 숲", "적만월(赤滿月)" }));

            foreach (var chapter in DefaultChapters.All)
            {
                Assert.That(chapter.Stages.Select(s => s.Type), Is.EqualTo(expectedLayout), chapter.Id);
            }
        }

        [Test]
        public void 챕터별_몬스터_풀과_보스는_원본과_같아야_한다()
        {
            Assert.That(DefaultChapters.Chapter1.MonsterPool.Select(m => m.Id),
                Is.EqualTo(new[] { "farmer", "hunter", "wolf" }));
            Assert.That(DefaultChapters.Chapter2.MonsterPool.Select(m => m.Id),
                Is.EqualTo(new[] { "wolf", "knight", "werewolf" }));
            Assert.That(DefaultChapters.Chapter3.MonsterPool.Select(m => m.Id),
                Is.EqualTo(new[] { "werewolf", "paladin", "lycan" }));

            Assert.That(DefaultChapters.Chapter1.Boss.Id, Is.EqualTo("priest"));
            Assert.That(DefaultChapters.Chapter2.Boss.Id, Is.EqualTo("vanhelsing"));
            Assert.That(DefaultChapters.Chapter3.Boss.Id, Is.EqualTo("werefenrir"));
        }

        [Test]
        public void 에피소드와_보스_클리어_규칙은_원본과_같아야_한다()
        {
            // 챕터 1, 2: 에피소드에서 스킬 획득, 보스 클리어 시 스킬 획득 + 체력 완전 회복
            foreach (var chapter in new[] { DefaultChapters.Chapter1, DefaultChapters.Chapter2 })
            {
                var episode = chapter.Stages[5];
                var boss = chapter.Stages[9];

                Assert.That(episode.GrantsSkill, Is.True, chapter.Id);
                Assert.That(episode.ClearReward, Is.Null, chapter.Id);
                Assert.That(boss.GrantsSkill, Is.True, chapter.Id);
                Assert.That(boss.ClearReward, Is.SameAs(DefaultRewards.MoonBlessing), chapter.Id);
            }

            // 챕터 3: 에피소드에서 강화, 보스 클리어에는 아무 규칙도 없음 (최종 보스)
            var chapter3 = DefaultChapters.Chapter3;
            Assert.That(chapter3.Stages[5].GrantsSkill, Is.False);
            Assert.That(chapter3.Stages[5].ClearReward, Is.SameAs(DefaultRewards.BloodMoonEnhance));
            Assert.That(chapter3.Stages[9].GrantsSkill, Is.False);
            Assert.That(chapter3.Stages[9].ClearReward, Is.Null);
        }
    }
}