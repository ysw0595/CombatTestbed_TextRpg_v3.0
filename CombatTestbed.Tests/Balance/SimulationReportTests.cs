using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Balance;

namespace CombatTestbed.Tests.Balance
{
    [TestFixture]
    public class SimulationReportTests
    {
        private static GameRecord Victory(int cleared, int actions, int maxHp, int attack)
        {
            return new GameRecord(GameOutcome.Victory, 2, 9, null, cleared, actions, maxHp, attack);
        }

        private static GameRecord Defeat(int chapter, int stage, string enemy, int cleared, int actions, int maxHp, int attack)
        {
            return new GameRecord(GameOutcome.Defeat, chapter, stage, enemy, cleared, actions, maxHp, attack);
        }

        // 승리 1판, 챕터 1 단계 10의 priest에게 2판, 챕터 2 단계 4의 knight에게 1판 패배
        private static SimulationReport SampleReport()
        {
            return new SimulationReport("테스트 전략", 42, new[]
            {
                Victory(30, 24, 300, 60),
                Defeat(0, 9, "priest", 9, 30, 100, 20),
                Defeat(0, 9, "priest", 9, 20, 120, 22),
                Defeat(1, 3, "knight", 14, 50, 180, 40)
            });
        }

        [Test]
        public void 클리어율과_평균값을_올바르게_계산해야_한다()
        {
            SimulationReport report = SampleReport();

            Assert.That(report.Games, Is.EqualTo(4));
            Assert.That(report.Victories, Is.EqualTo(1));
            Assert.That(report.Defeats, Is.EqualTo(3));
            Assert.That(report.WinRate, Is.EqualTo(0.25));
            Assert.That(report.AverageStagesCleared, Is.EqualTo(15.5));
            Assert.That(report.AverageActions, Is.EqualTo(31));
            Assert.That(report.AverageFinalMaxHp, Is.EqualTo(175));
            Assert.That(report.AverageFinalAttack, Is.EqualTo(35.5));
        }

        [Test]
        public void 패배_지점은_많이_진_곳이_앞에_오도록_묶어야_한다()
        {
            SimulationReport report = SampleReport();

            Assert.That(report.DefeatPoints.Count, Is.EqualTo(2));
            Assert.That(report.DefeatPoints[0].EnemyId, Is.EqualTo("priest"));
            Assert.That(report.DefeatPoints[0].Count, Is.EqualTo(2));
            Assert.That(report.DefeatPoints[1].EnemyId, Is.EqualTo("knight"));
            Assert.That(report.DefeatPoints[1].ChapterIndex, Is.EqualTo(1));
            Assert.That(report.DefeatPoints[1].StageIndex, Is.EqualTo(3));
        }

        [Test]
        public void 기록이_없어도_0으로_계산되고_예외가_나지_않아야_한다()
        {
            var report = new SimulationReport("빈 전략", 1, Enumerable.Empty<GameRecord>());

            Assert.That(report.WinRate, Is.EqualTo(0));
            Assert.That(report.AverageActions, Is.EqualTo(0));
            Assert.That(report.DefeatPoints, Is.Empty);
            Assert.DoesNotThrow(() => report.ToText());
        }

        [Test]
        public void 글로_출력하면_클리어율과_패배_지점이_들어_있어야_한다()
        {
            string text = SampleReport().ToText();

            Assert.That(text, Does.Contain("클리어율: 25.0%"));
            Assert.That(text, Does.Contain("테스트 전략"));
            Assert.That(text, Does.Contain("챕터 1 · 단계 10 · priest: 2판"));
        }
    }
}