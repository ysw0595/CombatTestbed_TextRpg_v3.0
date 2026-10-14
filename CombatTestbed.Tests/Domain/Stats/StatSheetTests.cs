using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Tests.Domain.Stats
{
    [TestFixture]
    public class StatSheetTests
    {
        [Test]
        public void 등록되지_않은_스탯을_조회하면_null_대신_0을_반환해야_한다()
        {
            var sheet = new StatSheet();

            Assert.That(sheet.GetStat(StatType.Defense), Is.Not.Null);
            Assert.That(sheet.GetValue(StatType.Defense), Is.EqualTo(0));
        }

        [Test]
        public void SetStat으로_새_스탯을_등록하면_조회할_수_있어야_한다()
        {
            var sheet = new StatSheet();

            sheet.SetStat(StatType.Attack, 25);

            Assert.That(sheet.GetValue(StatType.Attack), Is.EqualTo(25));
        }

        [Test]
        public void GetStat은_같은_타입에_대해_항상_같은_인스턴스를_반환해야_한다()
        {
            var sheet = new StatSheet();
            sheet.SetStat(StatType.MaxHp, 100);

            Assert.That(sheet.GetStat(StatType.MaxHp), Is.SameAs(sheet.GetStat(StatType.MaxHp)));
        }

        [Test]
        public void SetStat으로_재설정해도_기존_Stat_인스턴스와_수정치가_유지되어야_한다()
        {
            var sheet = new StatSheet();
            sheet.SetStat(StatType.Attack, 10);
            Stat before = sheet.GetStat(StatType.Attack);
            before.AddModifier(new StatModifier(StatModType.Flat, 5f, new object()));

            sheet.SetStat(StatType.Attack, 20);

            Assert.That(sheet.GetStat(StatType.Attack), Is.SameAs(before));
            Assert.That(sheet.GetValue(StatType.Attack), Is.EqualTo(25)); // 새 기본값 20 + 기존 수정치 5
        }
    }
}