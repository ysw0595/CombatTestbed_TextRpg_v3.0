using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Tests.Domain.Stats
{
    [TestFixture]
    public class StatTests
    {
        private StatModifier Flat(float value, object source = null)
        {
            return new StatModifier(StatModType.Flat, value, source ?? new object());
        }

        private StatModifier Percent(float value, object source = null)
        {
            return new StatModifier(StatModType.Percent, value, source ?? new object());
        }

        [Test]
        public void 수정치가_없으면_기본값이_그대로_최종값이어야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);

            Assert.That(stat.Value, Is.EqualTo(10));
        }

        [Test]
        public void Flat_수정치는_모두_합산되어야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);
            stat.AddModifier(Flat(5));
            stat.AddModifier(Flat(3));

            Assert.That(stat.Value, Is.EqualTo(18));
        }

        [Test]
        public void Percent_수정치는_먼저_합산한_뒤_한번에_곱해져야_한다()
        {
            var stat = new Stat(StatType.Attack, 100);
            stat.AddModifier(Percent(0.25f));
            stat.AddModifier(Percent(0.5f));

            // 곱연산이 아니라 가산: 100 * (1 + 0.25 + 0.5) = 175 (100*1.25*1.5=187 이 아님)
            Assert.That(stat.Value, Is.EqualTo(175));
        }

        [Test]
        public void 추가_순서와_무관하게_Flat이_먼저_합산된_뒤_Percent가_곱해져야_한다()
        {
            var stat = new Stat(StatType.Attack, 100);
            stat.AddModifier(Percent(0.5f)); // 일부러 Percent를 먼저 추가
            stat.AddModifier(Flat(20));

            // (100 + 20) * 1.5 = 180
            Assert.That(stat.Value, Is.EqualTo(180));
        }

        [Test]
        public void 소수점_결과는_0_5에서_올림으로_반올림되어야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);
            stat.AddModifier(Percent(0.25f)); // 10 * 1.25 = 12.5

            Assert.That(stat.Value, Is.EqualTo(13));
        }

        [Test]
        public void 값을_한번_조회한_뒤에도_수정치를_추가하면_최신_값이_반영되어야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);
            Assert.That(stat.Value, Is.EqualTo(10)); // 캐시가 채워짐

            stat.AddModifier(Flat(5));

            Assert.That(stat.Value, Is.EqualTo(15));
        }

        [Test]
        public void AddBaseValue는_기본값을_영구_성장시키고_기존_수정치도_유지해야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);
            stat.AddModifier(Flat(5));

            stat.AddBaseValue(10);

            Assert.That(stat.BaseValue, Is.EqualTo(20));
            Assert.That(stat.Value, Is.EqualTo(25)); // 20 + 5
        }

        [Test]
        public void 특정_주체의_수정치만_제거할_수_있어야_한다()
        {
            var sourceA = new object();
            var sourceB = new object();
            var stat = new Stat(StatType.Attack, 10);
            stat.AddModifier(Flat(5, sourceA));
            stat.AddModifier(Flat(10, sourceB));
            Assert.That(stat.Value, Is.EqualTo(25));

            bool removed = stat.RemoveModifiersFromSource(sourceA);

            Assert.That(removed, Is.True);
            Assert.That(stat.Value, Is.EqualTo(20));
            Assert.That(stat.RemoveModifiersFromSource(sourceA), Is.False); // 이미 제거됨
        }

        [Test]
        public void ClearModifiers는_모든_수정치를_제거해_기본값으로_되돌려야_한다()
        {
            var stat = new Stat(StatType.Attack, 10);
            stat.AddModifier(Flat(5));
            stat.AddModifier(Percent(0.5f));

            stat.ClearModifiers();

            Assert.That(stat.Value, Is.EqualTo(10));
            Assert.DoesNotThrow(() => stat.ClearModifiers()); // 빈 상태에서도 안전
        }
    }
}