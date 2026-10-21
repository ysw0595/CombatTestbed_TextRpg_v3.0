using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.Campaign
{
    [TestFixture]
    public class ChapterDefinitionTests
    {
        private static ActorDefinition Monster(string id)
        {
            return new ActorDefinition(id, id, ActorRole.Enemy, 10, 1);
        }

        [Test]
        public void 스킬_획득과_클리어_보상은_에피소드와_보스_단계에만_둘_수_있어야_한다()
        {
            var reward = DefaultRewards.MoonBlessing;

            Assert.DoesNotThrow(() => new StageDefinition(StageType.Episode, grantsSkill: true));
            Assert.DoesNotThrow(() => new StageDefinition(StageType.Boss, clearReward: reward));
            Assert.Throws<ArgumentException>(() => new StageDefinition(StageType.Prologue, grantsSkill: true));
            Assert.Throws<ArgumentException>(() => new StageDefinition(StageType.Battle, clearReward: reward));
        }

        [Test]
        public void 잘못된_입력으로는_챕터_정의를_만들_수_없어야_한다()
        {
            var battle = new StageDefinition(StageType.Battle);
            var bossStage = new StageDefinition(StageType.Boss);
            var monster = Monster("m");

            Assert.Throws<ArgumentException>(() => new ChapterDefinition("", "이름", new[] { battle }, new[] { monster }));
            Assert.Throws<ArgumentException>(() => new ChapterDefinition("id", " ", new[] { battle }, new[] { monster }));
            Assert.Throws<ArgumentNullException>(() => new ChapterDefinition("id", "이름", null));
            Assert.Throws<ArgumentException>(() => new ChapterDefinition("id", "이름", new StageDefinition[0]));
            Assert.Throws<ArgumentException>(() => new ChapterDefinition("id", "이름", new StageDefinition[] { null }));
            Assert.Throws<ArgumentException>(
                () => new ChapterDefinition("id", "이름", new[] { battle }, new ActorDefinition[] { null }));

            // 단계가 요구하는 데이터가 빠진 경우
            Assert.Throws<ArgumentException>(() => new ChapterDefinition("id", "이름", new[] { battle }));
            Assert.Throws<ArgumentException>(() => new ChapterDefinition("id", "이름", new[] { bossStage }));

            // 올바른 구성은 만들어져야 함
            Assert.DoesNotThrow(() => new ChapterDefinition(
                "id", "이름", new[] { battle, bossStage }, new[] { monster }, Monster("boss")));
        }

        [Test]
        public void 정의를_만든_뒤_원본_목록을_바꿔도_챕터의_구성은_변하지_않아야_한다()
        {
            var stages = new List<StageDefinition> { new StageDefinition(StageType.Battle) };
            var pool = new List<ActorDefinition> { Monster("a") };
            var chapter = new ChapterDefinition("id", "이름", stages, pool);

            stages.Add(new StageDefinition(StageType.Battle));
            pool.Add(Monster("b"));

            Assert.That(chapter.Stages.Count, Is.EqualTo(1));
            Assert.That(chapter.MonsterPool.Count, Is.EqualTo(1));
        }
    }
}