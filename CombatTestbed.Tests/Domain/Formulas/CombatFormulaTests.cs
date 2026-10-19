using NUnit.Framework;
using CombatTestbed.Core.Domain.Formulas;

namespace CombatTestbed.Tests.Domain.Formulas
{
    [TestFixture]
    public class CombatFormulaTests
    {
        [Test]
        public void 계수를_곱한_뒤_소수점은_버려야_한다()
        {
            Assert.That(CombatFormula.CalculateAmount(0, 17, 0.5f), Is.EqualTo(8));    // 8.5 -> 8
            Assert.That(CombatFormula.CalculateAmount(0, 19, 0.7f), Is.EqualTo(13));   // 13.3 -> 13
            Assert.That(CombatFormula.CalculateAmount(0, 300, 2.4f), Is.EqualTo(720));
        }

        [Test]
        public void 부동소수_표현_오차가_있는_계수도_기대한_값이_나와야_한다()
        {
            // float 그대로 곱하면 69.99999... 처럼 되어 버림 후 69가 될 수 있는 경우들
            Assert.That(CombatFormula.CalculateAmount(0, 100, 0.7f), Is.EqualTo(70));
            Assert.That(CombatFormula.CalculateAmount(0, 30, 0.7f), Is.EqualTo(21));
            Assert.That(CombatFormula.CalculateAmount(0, 100, 1.1f), Is.EqualTo(110));
            Assert.That(CombatFormula.CalculateAmount(0, 100, 0.08f), Is.EqualTo(8));
        }

        [Test]
        public void 고정값이_더해지고_결과가_음수면_0이어야_한다()
        {
            Assert.That(CombatFormula.CalculateAmount(5, 10, 1.0f), Is.EqualTo(15));
            Assert.That(CombatFormula.CalculateAmount(0, 10, -1.0f), Is.EqualTo(0));
            Assert.That(CombatFormula.CalculateAmount(3, 10, -1.0f), Is.EqualTo(0)); // 3 - 10 = -7 -> 0
        }

        [Test]
        public void 방어력을_차감하고_최소_1의_피해를_보장해야_한다()
        {
            Assert.That(CombatFormula.ApplyDefense(25, 10), Is.EqualTo(15));
            Assert.That(CombatFormula.ApplyDefense(10, 0), Is.EqualTo(10));
            Assert.That(CombatFormula.ApplyDefense(10, 50), Is.EqualTo(1));
            Assert.That(CombatFormula.ApplyDefense(0, 0), Is.EqualTo(1));
        }
    }
}