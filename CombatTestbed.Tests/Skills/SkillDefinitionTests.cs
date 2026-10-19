using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.Skills
{
    [TestFixture]
    public class SkillDefinitionTests
    {
        private static SkillEffectData AnyEffect()
        {
            return SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f);
        }

        [Test]
        public void 잘못된_입력으로는_스킬_정의를_만들_수_없어야_한다()
        {
            var effects = new[] { AnyEffect() };

            Assert.Throws<ArgumentException>(() => new SkillDefinition("", "이름", effects));
            Assert.Throws<ArgumentException>(() => new SkillDefinition("id", " ", effects));
            Assert.Throws<ArgumentNullException>(() => new SkillDefinition("id", "이름", null));
            Assert.Throws<ArgumentException>(() => new SkillDefinition("id", "이름", new SkillEffectData[0]));
            Assert.Throws<ArgumentException>(() => new SkillDefinition("id", "이름", new SkillEffectData[] { null }));
        }

        [Test]
        public void 정의를_만든_뒤_원본_목록을_바꿔도_정의의_효과는_변하지_않아야_한다()
        {
            var list = new List<SkillEffectData> { AnyEffect() };
            var definition = new SkillDefinition("id", "이름", list);

            list.Add(AnyEffect());

            Assert.That(definition.Effects.Count, Is.EqualTo(1));
        }

        [Test]
        public void 상태이상_효과_데이터는_스택이_1_미만이거나_종류가_없으면_만들_수_없어야_한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Poison, 0));
            Assert.Throws<ArgumentException>(
                () => SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.None, 1));
        }

        [Test]
        public void 고정량_효과는_양이_1_미만이면_만들_수_없어야_한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SkillEffectData.FlatHeal(EffectTarget.Self, 0));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, -1));
        }
    }
}