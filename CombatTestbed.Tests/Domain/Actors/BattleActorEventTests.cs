using System;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Domain.Actors
{
    [TestFixture]
    public class BattleActorEventTests
    {
        [Test]
        public void 피해를_입으면_DamageTaken이_실피해량과_함께_한번_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100, defense: 10);
            BattleActor raisedActor = null;
            int raisedAmount = 0;
            int raisedCount = 0;
            actor.DamageTaken += (a, amount) =>
            {
                raisedActor = a;
                raisedAmount = amount;
                raisedCount++;
            };

            actor.ApplyDamage(25); // 25 - 방어 10 = 15

            Assert.That(raisedCount, Is.EqualTo(1));
            Assert.That(raisedAmount, Is.EqualTo(15));
            Assert.That(raisedActor, Is.SameAs(actor));
        }

        [Test]
        public void 실피해가_없으면_DamageTaken이_발행되지_않아야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            int raisedCount = 0;
            actor.DamageTaken += (a, amount) => raisedCount++;

            actor.ApplyTrueDamage(0);
            actor.ApplyTrueDamage(-3);

            Assert.That(raisedCount, Is.EqualTo(0));
        }

        [Test]
        public void 치명적인_피해를_입으면_DamageTaken_다음에_Died가_순서대로_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 10, defense: 0);
            var log = new EventLog();
            actor.DamageTaken += (a, amount) => log.Add($"DamageTaken:{amount}");
            actor.Died += a => log.Add("Died");

            actor.ApplyDamage(25); // 오버킬이지만 실피해는 남은 체력 10

            Assert.That(log.Entries, Is.EqualTo(new[] { "DamageTaken:10", "Died" }));
        }

        [Test]
        public void 사망_후_추가_피해를_받아도_Died와_DamageTaken은_더_발행되지_않아야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 10);
            var log = new EventLog();
            actor.DamageTaken += (a, amount) => log.Add($"DamageTaken:{amount}");
            actor.Died += a => log.Add("Died");

            actor.ApplyDamage(10);     // 사망
            actor.ApplyDamage(5);      // 사망 후 추가 피해
            actor.ApplyTrueDamage(5);  // 사망 후 고정 피해

            Assert.That(log.Entries, Is.EqualTo(new[] { "DamageTaken:10", "Died" }));
        }

        [Test]
        public void 이벤트_핸들러에서는_이미_갱신된_체력과_사망_상태를_읽을_수_있어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100, defense: 0);
            int hpSeenInDamageHandler = -1;
            bool deadSeenInDiedHandler = false;
            actor.DamageTaken += (a, amount) => hpSeenInDamageHandler = a.Health.CurrentHp;
            actor.Died += a => deadSeenInDiedHandler = a.Health.IsDead;

            actor.ApplyDamage(15);
            Assert.That(hpSeenInDamageHandler, Is.EqualTo(85));

            actor.ApplyDamage(85); // 사망
            Assert.That(hpSeenInDamageHandler, Is.EqualTo(0));
            Assert.That(deadSeenInDiedHandler, Is.True);
        }

        [Test]
        public void 회복하면_Healed가_오버힐을_제외한_실제_회복량과_함께_한번_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            int healedAmount = 0;
            int raisedCount = 0;
            actor.Healed += (a, amount) =>
            {
                healedAmount = amount;
                raisedCount++;
            };
            actor.ApplyTrueDamage(30); // 체력 70

            actor.ApplyHeal(50); // 실제로는 30만 회복

            Assert.That(raisedCount, Is.EqualTo(1));
            Assert.That(healedAmount, Is.EqualTo(30));
        }

        [Test]
        public void 이미_체력이_가득_찼다면_Healed가_발행되지_않아야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            int raisedCount = 0;
            actor.Healed += (a, amount) => raisedCount++;

            actor.ApplyHeal(10);

            Assert.That(raisedCount, Is.EqualTo(0));
        }

        [Test]
        public void 구독을_해제하면_이후_이벤트를_받지_않아야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            int raisedCount = 0;
            Action<BattleActor, int> handler = (a, amount) => raisedCount++;
            actor.DamageTaken += handler;

            actor.ApplyDamage(5);
            actor.DamageTaken -= handler; // 같은 핸들러 참조로 해제
            actor.ApplyDamage(5);

            Assert.That(raisedCount, Is.EqualTo(1));
        }
    }
}