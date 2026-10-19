using System;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Skills
{
    [TestFixture]
    public class SkillEffectExecutorTests
    {
        [Test]
        public void 자신을_대상으로_하는_효과는_상대_없이도_실행되어야_한다()
        {
            var actor = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            actor.ApplyTrueDamage(30); // 체력 70

            EffectOutcome outcome = SkillEffectExecutor.Execute(
                SkillEffectData.FlatHeal(EffectTarget.Self, 10), actor, null);

            Assert.That(outcome.Kind, Is.EqualTo(SkillEffectKind.Heal));
            Assert.That(outcome.Receiver, Is.SameAs(actor));
            Assert.That(outcome.Amount, Is.EqualTo(10));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(80));
        }

        [Test]
        public void 상대를_대상으로_하는_효과에_상대가_없으면_예외가_발생해야_한다()
        {
            var actor = TestActorFactory.Create(name: "흡혈귀");
            var effect = SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f);

            Assert.Throws<ArgumentNullException>(() => SkillEffectExecutor.Execute(effect, actor, null));
        }
    }
}