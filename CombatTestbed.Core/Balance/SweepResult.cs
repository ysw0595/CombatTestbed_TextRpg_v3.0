#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 여러 값을 차례로 시험한 스윕의 전체 결과입니다. "값이 바뀌면 클리어율이 어떻게 변하는가"를 표로 보여 줍니다.
    /// </summary>
    public sealed class SweepResult
    {
        public string ParameterName { get; }
        public string PolicyName { get; }
        public int GamesPerValue { get; }
        public int Seed { get; }
        public IReadOnlyList<SweepPoint> Points { get; }

        public SweepResult(
            string parameterName, string policyName, int gamesPerValue, int seed, IEnumerable<SweepPoint> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            PolicyName = policyName ?? throw new ArgumentNullException(nameof(policyName));
            GamesPerValue = gamesPerValue;
            Seed = seed;
            Points = new List<SweepPoint>(points).AsReadOnly();
        }

        public string ToText()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"=== 파라미터 스윕: {ParameterName} | 전략: {PolicyName} | 값마다 {GamesPerValue}판 (시드 {Seed}) ===");
            sb.AppendLine("   값 | 클리어율 | 평균 클리어 단계 | 가장 많이 진 곳");

            foreach (SweepPoint point in Points)
            {
                SimulationReport report = point.Report;

                string worst = report.DefeatPoints.Count > 0
                    ? $"챕터 {report.DefeatPoints[0].ChapterIndex + 1} · 단계 {report.DefeatPoints[0].StageIndex + 1} · {report.DefeatPoints[0].EnemyId}"
                    : "-";

                sb.AppendLine(
                    $"{point.Value.ToString().PadLeft(5)} | " +
                    $"{SimulationReport.Percent(report.WinRate).PadLeft(8)} | " +
                    $"{SimulationReport.Number(report.AverageStagesCleared).PadLeft(16)} | {worst}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}