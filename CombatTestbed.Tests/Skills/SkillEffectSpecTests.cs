using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.Skills
{
    [TestFixture]
    public class SkillEffectSpecTests
    {
        // 효과 데이터의 모든 필드가 같은지 비교합니다.
        private static void AssertSame(SkillEffectData expected, SkillEffectData actual)
        {
            Assert.That(actual.Kind, Is.EqualTo(expected.Kind));
            Assert.That(actual.Target, Is.EqualTo(expected.Target));
            Assert.That(actual.ScalingStat, Is.EqualTo(expected.ScalingStat));
            Assert.That(actual.Coefficient, Is.EqualTo(expected.Coefficient));
            Assert.That(actual.FlatValue, Is.EqualTo(expected.FlatValue));
            Assert.That(actual.DamageType, Is.EqualTo(expected.DamageType));
            Assert.That(actual.StatusType, Is.EqualTo(expected.StatusType));
            Assert.That(actual.Stacks, Is.EqualTo(expected.Stacks));
        }

        [Test]
        public void 피해_스펙은_같은_값의_피해_효과로_변환되어야_한다()
        {
            var spec = new SkillEffectSpec
            {
                Kind = SkillEffectKind.Damage,
                Target = EffectTarget.Opponent,
                ScalingStat = StatType.Attack,
                Coefficient = 2.4f,
                FlatValue = 3,
                DamageType = DamageType.True
            };

            SkillEffectData data = spec.ToData();

            AssertSame(SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.4f, DamageType.True, 3), data);
        }

        [Test]
        public void 회복_스펙은_같은_값의_회복_효과로_변환되어야_한다()
        {
            var spec = new SkillEffectSpec
            {
                Kind = SkillEffectKind.Heal,
                Target = EffectTarget.Self,
                ScalingStat = StatType.Attack,
                Coefficient = 0.08f,
                FlatValue = 5
            };

            SkillEffectData data = spec.ToData();

            AssertSame(SkillEffectData.Heal(EffectTarget.Self, StatType.Attack, 0.08f, 5), data);
        }

        [Test]
        public void 상태이상_스펙은_종류와_스택이_같은_효과로_변환되어야_한다()
        {
            var spec = new SkillEffectSpec
            {
                Kind = SkillEffectKind.ApplyStatus,
                Target = EffectTarget.Opponent,
                StatusType = StatusEffectType.Bleed,
                Stacks = 10
            };

            SkillEffectData data = spec.ToData();

            AssertSame(SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Bleed, 10), data);
        }

        [Test]
        public void 스탯증가_스펙은_같은_값의_스탯증가_효과로_변환되어야_한다()
        {
            var spec = new SkillEffectSpec
            {
                Kind = SkillEffectKind.StatGain,
                Target = EffectTarget.Self,
                ScalingStat = StatType.MaxHp,
                Coefficient = 0f,
                FlatValue = 10
            };

            SkillEffectData data = spec.ToData();

            AssertSame(SkillEffectData.StatGain(EffectTarget.Self, StatType.MaxHp, 0f, 10), data);
        }

        [Test]
        public void 상태이상_스택이_0이면_변환할_때_예외를_던져야_한다()
        {
            var spec = new SkillEffectSpec
            {
                Kind = SkillEffectKind.ApplyStatus,
                Target = EffectTarget.Opponent,
                StatusType = StatusEffectType.Poison,
                Stacks = 0
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToData());
        }

        [Test]
        public void 알_수_없는_효과_종류면_InvalidOperationException을_던져야_한다()
        {
            var spec = new SkillEffectSpec { Kind = (SkillEffectKind)99 };

            Assert.Throws<InvalidOperationException>(() => spec.ToData());
        }

        [Test]
        public void null_효과_데이터로는_스펙을_만들_수_없어야_한다()
        {
            Assert.Throws<ArgumentNullException>(() => SkillEffectSpec.From(null!));
        }

        [Test]
        public void 모든_기본_스킬의_효과는_스펙으로_바꿨다가_되돌려도_같아야_한다()
        {
            var skills = DefaultSkills.VampireSkills.Concat(new[]
            {
                DefaultSkills.PriestReadSpell, DefaultSkills.PriestCross, DefaultSkills.PriestPray,
                DefaultSkills.VanHelsingSilverArrow, DefaultSkills.VanHelsingContinuousArrow,
                DefaultSkills.VanHelsingPoisonArrow,
                DefaultSkills.WereFenrirMoonLeap, DefaultSkills.WereFenrirScratch, DefaultSkills.WereFenrirBite
            });

            foreach (var skill in skills)
            {
                foreach (SkillEffectData effect in skill.Effects)
                {
                    AssertSame(effect, SkillEffectSpec.From(effect).ToData());
                }
            }
        }

        [Test]
        public void 모든_기본_보상의_효과는_스펙으로_바꿨다가_되돌려도_같아야_한다()
        {
            var rewards = new[]
            {
                DefaultRewards.Farmer, DefaultRewards.Hunter, DefaultRewards.Wolf, DefaultRewards.Knight,
                DefaultRewards.Werewolf, DefaultRewards.Paladin, DefaultRewards.Lycan,
                DefaultRewards.MoonBlessing, DefaultRewards.BloodMoonEnhance
            };

            foreach (var reward in rewards)
            {
                foreach (SkillEffectData effect in reward.Effects)
                {
                    AssertSame(effect, SkillEffectSpec.From(effect).ToData());
                }
            }
        }
    }
}