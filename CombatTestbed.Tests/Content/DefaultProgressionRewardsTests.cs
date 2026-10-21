using NUnit.Framework;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Content
{
    [TestFixture]
    public class DefaultProgressionRewardsTests
    {
        [Test]
        public void 달의_가호는_체력을_최대로_회복하고_이미_가득_차_있으면_회복량이_0이어야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            player.ApplyTrueDamage(60); // 체력 40

            var outcomes = RewardApplier.Apply(DefaultRewards.MoonBlessing, player);

            Assert.That(player.Health.CurrentHp, Is.EqualTo(100));
            Assert.That(outcomes[0].Amount, Is.EqualTo(60)); // 실제로 회복된 양

            var again = RewardApplier.Apply(DefaultRewards.MoonBlessing, player);
            Assert.That(again[0].Amount, Is.EqualTo(0));
        }

        [Test]
        public void 붉은_달_강화는_최대체력과_공격력을_두배로_하고_현재체력은_늘어난_최대체력만큼_함께_올라야_한다()
        {
            var player = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 30);
            player.ApplyTrueDamage(40); // 체력 60

            RewardApplier.Apply(DefaultRewards.BloodMoonEnhance, player);

            Assert.That(player.Health.MaxHp, Is.EqualTo(200));
            Assert.That(player.Stats.GetValue(StatType.Attack), Is.EqualTo(60));
            // 의도된 차이: 원본은 현재 체력도 x2(120)였으나, 새 구조는 증가한 최대 체력(+100)만큼 올림
            Assert.That(player.Health.CurrentHp, Is.EqualTo(160));
        }
    }
}