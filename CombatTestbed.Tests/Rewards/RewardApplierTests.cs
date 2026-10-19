using System;
using NUnit.Framework;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Rewards
{
    [TestFixture]
    public class RewardApplierTests
    {
        [Test]
        public void 기본_보상_7종은_원본_게임의_보상_수치와_같아야_한다()
        {
            // (보상, 최대 체력 증가, 공격력 증가, 회복 요청량) - 원본 Monster.ShowDisappear의 수치
            var cases = new[]
            {
                (reward: DefaultRewards.Farmer,   maxHpGain: 1,  attackGain: 0, heal: 0),
                (reward: DefaultRewards.Hunter,   maxHpGain: 0,  attackGain: 1, heal: 0),
                (reward: DefaultRewards.Wolf,     maxHpGain: 0,  attackGain: 0, heal: 20),
                (reward: DefaultRewards.Knight,   maxHpGain: 10, attackGain: 0, heal: 0),
                (reward: DefaultRewards.Werewolf, maxHpGain: 0,  attackGain: 4, heal: 30),
                (reward: DefaultRewards.Paladin,  maxHpGain: 20, attackGain: 0, heal: 0),
                (reward: DefaultRewards.Lycan,    maxHpGain: 0,  attackGain: 8, heal: 40),
            };

            foreach (var c in cases)
            {
                // 최대 체력 1000, 현재 체력 100 (회복이 상한에 걸리지 않도록 넉넉히)
                var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 1000, attack: 10);
                player.ApplyTrueDamage(900);

                RewardApplier.Apply(c.reward, player);

                string label = $"보상: {c.reward.Id}";
                Assert.That(player.Health.MaxHp, Is.EqualTo(1000 + c.maxHpGain), label);
                Assert.That(player.Stats.GetValue(StatType.Attack), Is.EqualTo(10 + c.attackGain), label);
                // 최대 체력 증가분은 현재 체력도 함께 오르고, 그 뒤에 회복이 더해짐
                Assert.That(player.Health.CurrentHp, Is.EqualTo(100 + c.maxHpGain + c.heal), label);
            }
        }

        [Test]
        public void 적용_결과에는_효과_순서대로_종류와_올라간_스탯과_실제_양이_담겨야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 1000, attack: 10);
            player.ApplyTrueDamage(900);

            var outcomes = RewardApplier.Apply(DefaultRewards.Werewolf, player);

            Assert.That(outcomes.Count, Is.EqualTo(2));
            Assert.That(outcomes[0].Kind, Is.EqualTo(SkillEffectKind.StatGain));
            Assert.That(outcomes[0].Stat, Is.EqualTo(StatType.Attack));
            Assert.That(outcomes[0].Amount, Is.EqualTo(4));
            Assert.That(outcomes[1].Kind, Is.EqualTo(SkillEffectKind.Heal));
            Assert.That(outcomes[1].Amount, Is.EqualTo(30));
        }

        [Test]
        public void 회복_보상은_최대_체력을_넘지_않고_실제_회복량만_결과에_담아야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            player.ApplyTrueDamage(10); // 체력 90

            var outcomes = RewardApplier.Apply(DefaultRewards.Wolf, player); // 회복 20 요청

            Assert.That(player.Health.CurrentHp, Is.EqualTo(100));
            Assert.That(outcomes[0].Amount, Is.EqualTo(10)); // 실제로 회복된 양
        }

        [Test]
        public void 사망한_액터에게는_회복_보상이_적용되지_않고_부활하지_않아야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            player.ApplyTrueDamage(100); // 사망

            var outcomes = RewardApplier.Apply(DefaultRewards.Wolf, player);

            Assert.That(outcomes[0].Amount, Is.EqualTo(0));
            Assert.That(player.Health.IsDead, Is.True);
        }

        [Test]
        public void 보상이나_받는_액터가_null이면_예외가_발생해야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀");

            Assert.Throws<ArgumentNullException>(() => RewardApplier.Apply(null, player));
            Assert.Throws<ArgumentNullException>(() => RewardApplier.Apply(DefaultRewards.Wolf, null));
        }
    }
}