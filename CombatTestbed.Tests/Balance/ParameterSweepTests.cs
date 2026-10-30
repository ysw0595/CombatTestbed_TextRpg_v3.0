using System;
using NUnit.Framework;
using CombatTestbed.Core.Balance;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Tests.Balance
{
    [TestFixture]
    public class ParameterSweepTests
    {
        // 공격력 값 하나를 받아 "그 공격력의 용사로 캠페인을 만드는 팩토리"를 돌려줍니다.
        private static Func<IRandomSource, CampaignManager> AttackFactory(int attack)
        {
            var hero = new ActorDefinition("hero", "용사", ActorRole.Player, 100, attack);
            return random => DefaultCampaign.Create(random, hero);
        }

        private static SweepResult RunSweep(params int[] values)
        {
            return ParameterSweep.Run("공격력", values, AttackFactory, new FirstSkillPolicy(), 10, 1);
        }

        [Test]
        public void 값마다_결과가_하나씩_주어진_순서대로_나와야_한다()
        {
            SweepResult result = RunSweep(1, 100000);

            Assert.That(result.Points.Count, Is.EqualTo(2));
            Assert.That(result.Points[0].Value, Is.EqualTo(1));
            Assert.That(result.Points[1].Value, Is.EqualTo(100000));
        }

        [Test]
        public void 값이_달라지면_클리어율도_그에_맞게_달라져야_한다()
        {
            SweepResult result = RunSweep(1, 100000);

            Assert.That(result.Points[0].Report.WinRate, Is.EqualTo(0));
            Assert.That(result.Points[1].Report.WinRate, Is.EqualTo(1));
        }

        [Test]
        public void 값_목록이_비어_있으면_ArgumentException을_던져야_한다()
        {
            Assert.Throws<ArgumentException>(() => RunSweep());
        }

        [Test]
        public void 표_글에_파라미터_이름과_값별_클리어율이_들어_있어야_한다()
        {
            string text = RunSweep(1, 100000).ToText();

            Assert.That(text, Does.Contain("파라미터 스윕: 공격력"));
            Assert.That(text, Does.Contain("100000"));
            Assert.That(text, Does.Contain("100.0%"));
            Assert.That(text, Does.Contain("0.0%"));
        }
    }
}