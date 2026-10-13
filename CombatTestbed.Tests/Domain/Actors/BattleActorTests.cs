using System;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Tests.Domain.Actors
{
    [TestFixture]
    public class BattleActorTests
    {
        private BattleActor CreateTestActor(int maxHp, int defense)
        {
            var stats = new StatSheet();
            stats.SetStat(StatType.MaxHp, maxHp);
            stats.SetStat(StatType.Defense, defense);
            return new BattleActor(Guid.NewGuid().ToString(), "테스트 액터", ActorRole.Enemy, stats);
        }

        [Test]
        public void ApplyDamage는_방어력을_차감한_실피해를_반환하고_체력에_반영해야_한다()
        {
            var actor = CreateTestActor(maxHp: 100, defense: 10);

            int dealt = actor.ApplyDamage(25);

            Assert.That(dealt, Is.EqualTo(15));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(85));
        }

        [Test]
        public void 방어력이_피해보다_커도_최소_1의_피해는_들어가야_한다()
        {
            var actor = CreateTestActor(maxHp: 100, defense: 50);

            int dealt = actor.ApplyDamage(10);

            Assert.That(dealt, Is.EqualTo(1));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(99));
        }

        [Test]
        public void 남은_체력보다_큰_피해를_입으면_반환값은_실제로_깎인_체력만큼이어야_한다()
        {
            var actor = CreateTestActor(maxHp: 10, defense: 0);

            int dealt = actor.ApplyDamage(25);

            Assert.That(dealt, Is.EqualTo(10)); // 오버킬 분량은 제외
            Assert.That(actor.Health.IsDead, Is.True);
        }

        [Test]
        public void ApplyTrueDamage는_방어력을_무시하고_실피해를_반환해야_한다()
        {
            var actor = CreateTestActor(maxHp: 100, defense: 50);

            int dealt = actor.ApplyTrueDamage(7);

            Assert.That(dealt, Is.EqualTo(7));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(93));
        }

        [Test]
        public void 이미_사망한_액터에게는_어떤_피해를_줘도_0을_반환해야_한다()
        {
            var actor = CreateTestActor(maxHp: 10, defense: 0);
            actor.ApplyDamage(10); // 사망

            Assert.That(actor.ApplyDamage(5), Is.EqualTo(0));
            Assert.That(actor.ApplyTrueDamage(5), Is.EqualTo(0));
        }

        [Test]
        public void ApplyHeal은_오버힐을_제외한_실제_회복량을_반환해야_한다()
        {
            var actor = CreateTestActor(maxHp: 100, defense: 0);
            actor.ApplyTrueDamage(30); // 체력 70

            int healed = actor.ApplyHeal(50);

            Assert.That(healed, Is.EqualTo(30));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void MaxHp가_설정되지_않은_StatSheet로는_액터를_생성할_수_없어야_한다()
        {
            var stats = new StatSheet(); // MaxHp 미설정(기본값 0)

            Assert.Throws<ArgumentException>(() =>
                new BattleActor("id", "이름", ActorRole.Enemy, stats));
        }
    }
}