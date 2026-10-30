using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Balance;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.Balance
{
    [TestFixture]
    public class CampaignSimulatorTests
    {
        private static ActorDefinition Hero(int maxHp, int attack)
        {
            return new ActorDefinition("hero", "용사", ActorRole.Player, maxHp, attack);
        }

        private static CampaignSimulator CreateSimulator(
            ActorDefinition hero, ISkillPolicy policy, int maxActions = CampaignSimulator.DefaultMaxActionsPerGame)
        {
            return new CampaignSimulator(random => DefaultCampaign.Create(random, hero), policy, maxActions);
        }

        private static string[] Summarize(SimulationReport report)
        {
            return report.Records
                .Select(r => $"{r.Outcome}/{r.StagesCleared}/{r.Actions}/{r.FinalMaxHp}/{r.FinalAttack}")
                .ToArray();
        }

        [Test]
        public void 압도적으로_강하면_모든_판을_클리어하고_단계와_행동_수가_정해진_값이어야_한다()
        {
            var simulator = CreateSimulator(Hero(100000, 100000), new FirstSkillPolicy());

            SimulationReport report = simulator.Run(20, 1);

            Assert.That(report.Victories, Is.EqualTo(20));
            Assert.That(report.Defeats, Is.EqualTo(0));
            Assert.That(report.AverageStagesCleared, Is.EqualTo(30)); // 3개 챕터 x 10단계
            Assert.That(report.AverageActions, Is.EqualTo(24));       // 챕터마다 전투 8번 x 3, 한 방에 끝남
        }

        [Test]
        public void 너무_약하면_모든_판이_첫_전투에서_패배해야_한다()
        {
            var simulator = CreateSimulator(Hero(1, 1), new FirstSkillPolicy());

            SimulationReport report = simulator.Run(20, 1);

            Assert.That(report.Defeats, Is.EqualTo(20));
            Assert.That(report.DefeatPoints.All(p => p.ChapterIndex == 0 && p.StageIndex == 1), Is.True);
            Assert.That(report.DefeatPoints.Sum(p => p.Count), Is.EqualTo(20));
        }

        [Test]
        public void 행동_상한을_넘으면_시간_초과로_기록해야_한다()
        {
            var simulator = CreateSimulator(Hero(100, 10), new FirstSkillPolicy(), maxActions: 3);

            SimulationReport report = simulator.Run(10, 1);

            Assert.That(report.Stalls, Is.EqualTo(10));
            Assert.That(report.Victories, Is.EqualTo(0));
        }

        [Test]
        public void 같은_시드로_다시_돌리면_모든_판의_결과가_같아야_한다()
        {
            var first = Summarize(CreateSimulator(Hero(100, 10), new RandomSkillPolicy()).Run(50, 42));
            var second = Summarize(CreateSimulator(Hero(100, 10), new RandomSkillPolicy()).Run(50, 42));

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void 판_수가_0_이하면_ArgumentOutOfRangeException을_던져야_한다()
        {
            var simulator = CreateSimulator(Hero(100, 10), new FirstSkillPolicy());

            Assert.Throws<ArgumentOutOfRangeException>(() => simulator.Run(0, 1));
        }

        [Test]
        public void 생성_인자가_잘못되면_예외를_던져야_한다()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignSimulator(null!, new FirstSkillPolicy()));
            Assert.Throws<ArgumentNullException>(() =>
                new CampaignSimulator(random => DefaultCampaign.Create(random), null!));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CampaignSimulator(random => DefaultCampaign.Create(random), new FirstSkillPolicy(), 0));
        }

        [Test]
        public void 캠페인_팩토리가_null을_돌려주면_InvalidOperationException을_던져야_한다()
        {
            var simulator = new CampaignSimulator(random => null!, new FirstSkillPolicy());

            Assert.Throws<InvalidOperationException>(() => simulator.Run(1, 1));
        }
    }
}