using System;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Tests.Domain.Stats
{
    [TestFixture]
    public class HealthResourceTests
    {
        private Stat MakeMaxHpStat(int baseValue)
        {
            return new Stat(StatType.MaxHp, baseValue);
        }

        [Test]
        public void 생성_직후_현재체력은_최대체력과_같고_생존_상태여야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));

            Assert.That(health.MaxHp, Is.EqualTo(100));
            Assert.That(health.CurrentHp, Is.EqualTo(100));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void 최대체력이_0이하이거나_null이면_생성할_수_없어야_한다()
        {
            Assert.Throws<ArgumentException>(() => new HealthResource(MakeMaxHpStat(0)));
            Assert.Throws<ArgumentException>(() => new HealthResource(MakeMaxHpStat(-5)));
            Assert.Throws<ArgumentNullException>(() => new HealthResource(null));
        }

        [Test]
        public void TakeDamage는_실제로_깎인_양을_반환해야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));

            int dealt = health.TakeDamage(30);

            Assert.That(dealt, Is.EqualTo(30));
            Assert.That(health.CurrentHp, Is.EqualTo(70));
        }

        [Test]
        public void 남은_체력보다_큰_피해는_남은_체력만큼만_반환하고_사망해야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));

            int dealt = health.TakeDamage(150);

            Assert.That(dealt, Is.EqualTo(100)); // 오버킬 분량 제외
            Assert.That(health.CurrentHp, Is.EqualTo(0));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void 무효한_피해이거나_이미_사망했다면_TakeDamage는_0을_반환해야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));

            Assert.That(health.TakeDamage(0), Is.EqualTo(0));
            Assert.That(health.TakeDamage(-5), Is.EqualTo(0));
            Assert.That(health.CurrentHp, Is.EqualTo(100));

            health.TakeDamage(100); // 사망
            Assert.That(health.TakeDamage(10), Is.EqualTo(0));
        }

        [Test]
        public void Heal은_오버힐을_제외한_실제_회복량을_반환해야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));
            health.TakeDamage(30); // 70

            int healed = health.Heal(50);

            Assert.That(healed, Is.EqualTo(30));
            Assert.That(health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void 사망한_상태에서는_Heal이_0을_반환하고_부활하지_않아야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));
            health.TakeDamage(100);

            int healed = health.Heal(10);

            Assert.That(healed, Is.EqualTo(0));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void 디버프로_최대체력이_줄면_현재체력은_새_상한으로_보여야_한다()
        {
            var maxHpStat = MakeMaxHpStat(100);
            var health = new HealthResource(maxHpStat);

            maxHpStat.AddModifier(new StatModifier(StatModType.Flat, -40f, new object()));

            Assert.That(health.MaxHp, Is.EqualTo(60));
            Assert.That(health.CurrentHp, Is.EqualTo(60));

            // 이후 피해는 줄어든 상한 기준으로 계산됨
            int dealt = health.TakeDamage(10);
            Assert.That(dealt, Is.EqualTo(10));
            Assert.That(health.CurrentHp, Is.EqualTo(50));
        }

        [Test]
        public void IncreaseMaxHp는_최대체력과_현재체력을_함께_증가시켜야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));
            health.TakeDamage(30); // 70

            health.IncreaseMaxHp(10);

            Assert.That(health.MaxHp, Is.EqualTo(110));
            Assert.That(health.CurrentHp, Is.EqualTo(80));
        }

        [Test]
        public void 사망한_상태에서_IncreaseMaxHp는_최대체력만_늘리고_부활시키지_않아야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));
            health.TakeDamage(100);

            health.IncreaseMaxHp(10);

            Assert.That(health.MaxHp, Is.EqualTo(110));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void FullHeal은_최대체력으로_복구해야_한다()
        {
            var health = new HealthResource(MakeMaxHpStat(100));
            health.TakeDamage(60);

            health.FullHeal();

            Assert.That(health.CurrentHp, Is.EqualTo(100));
        }
    }
}