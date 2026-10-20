using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.Definitions
{
    [TestFixture]
    public class ActorDefinitionTests
    {
        private static SkillDefinition AnySkill()
        {
            return new SkillDefinition("s", "스킬", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
            });
        }

        [Test]
        public void 잘못된_입력으로는_액터_정의를_만들_수_없어야_한다()
        {
            Assert.Throws<ArgumentException>(() => new ActorDefinition("", "이름", ActorRole.Enemy, 10, 1));
            Assert.Throws<ArgumentException>(() => new ActorDefinition("id", " ", ActorRole.Enemy, 10, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActorDefinition("id", "이름", ActorRole.Enemy, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActorDefinition("id", "이름", ActorRole.Enemy, 10, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActorDefinition("id", "이름", ActorRole.Enemy, 10, 1, defense: -1));
            Assert.Throws<ArgumentException>(() => new ActorDefinition(
                "id", "이름", ActorRole.Enemy, 10, 1, behaviorSkills: new SkillDefinition[] { null }));
        }

        [Test]
        public void 행동_스킬_목록은_생략하면_비어_있고_원본_목록을_바꿔도_정의는_변하지_않아야_한다()
        {
            var withoutSkills = new ActorDefinition("id", "이름", ActorRole.Enemy, 10, 1);
            var list = new List<SkillDefinition> { AnySkill() };
            var withSkills = new ActorDefinition("id", "이름", ActorRole.Enemy, 10, 1, behaviorSkills: list);

            list.Add(AnySkill());

            Assert.That(withoutSkills.BehaviorSkills.Count, Is.EqualTo(0));
            Assert.That(withSkills.BehaviorSkills.Count, Is.EqualTo(1));
        }
    }
}