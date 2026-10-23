using System;
using NUnit.Framework;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Tests.Domain.StatusEffects
{
    [TestFixture]
    public class StatusEffectFactoryTests
    {
        [Test]
        public void 종류에_맞는_효과를_지정한_스택으로_만들어야_한다()
        {
            IStatusEffect poison = StatusEffectFactory.Create(StatusEffectType.Poison, 3);
            IStatusEffect bleed = StatusEffectFactory.Create(StatusEffectType.Bleed, 10);
            IStatusEffect paralysis = StatusEffectFactory.Create(StatusEffectType.Paralysis, 1);

            Assert.That(poison, Is.TypeOf<PoisonEffect>());
            Assert.That(poison.Stacks, Is.EqualTo(3));
            Assert.That(bleed, Is.TypeOf<BleedEffect>());
            Assert.That(bleed.Stacks, Is.EqualTo(10));
            Assert.That(paralysis, Is.TypeOf<ParalysisEffect>());
            Assert.That(paralysis.Stacks, Is.EqualTo(1));
        }

        [Test]
        public void 지원하지_않는_종류는_예외가_발생해야_한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => StatusEffectFactory.Create(StatusEffectType.None, 1));
        }

        [Test]
        public void 혈석_종류는_혈석_효과를_지정한_스택으로_만들어야_한다()
        {
            IStatusEffect effect = StatusEffectFactory.Create(StatusEffectType.BloodDiamond, 2);

            Assert.That(effect, Is.TypeOf<BloodDiamondEffect>());
            Assert.That(effect.Stacks, Is.EqualTo(2));
        }
    }
}