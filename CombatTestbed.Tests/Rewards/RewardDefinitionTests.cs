using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.Rewards
{
    [TestFixture]
    public class RewardDefinitionTests
    {
        [Test]
        public void 잘못된_입력으로는_보상_정의를_만들_수_없어야_한다()
        {
            var heal = SkillEffectData.FlatHeal(EffectTarget.Self, 10);
            var opponentDamage = SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f);

            Assert.Throws<ArgumentException>(() => new RewardDefinition("", new[] { heal }));
            Assert.Throws<ArgumentNullException>(() => new RewardDefinition("id", null));
            Assert.Throws<ArgumentException>(() => new RewardDefinition("id", new SkillEffectData[0]));
            Assert.Throws<ArgumentException>(() => new RewardDefinition("id", new SkillEffectData[] { null }));
            // 보상은 받는 쪽(Self)에게만 적용되어야 함
            Assert.Throws<ArgumentException>(() => new RewardDefinition("id", new[] { opponentDamage }));
        }

        [Test]
        public void 정의를_만든_뒤_원본_목록을_바꿔도_보상의_효과는_변하지_않아야_한다()
        {
            var list = new List<SkillEffectData> { SkillEffectData.FlatHeal(EffectTarget.Self, 10) };
            var reward = new RewardDefinition("id", list);

            list.Add(SkillEffectData.FlatHeal(EffectTarget.Self, 20));

            Assert.That(reward.Effects.Count, Is.EqualTo(1));
        }
    }
}