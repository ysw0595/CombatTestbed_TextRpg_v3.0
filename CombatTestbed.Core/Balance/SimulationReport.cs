#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 여러 판의 기록을 모아 클리어율, 평균, 패배 지점을 계산하는 결과 보고서입니다.
    /// </summary>
    public sealed class SimulationReport
    {
        public string PolicyName { get; }
        public int Seed { get; }
        public IReadOnlyList<GameRecord> Records { get; }

        public int Games => Records.Count;
        public int Victories { get; }
        public int Defeats { get; }
        public int Stalls { get; }

        /// <summary>패배 지점 집계. 많이 진 곳이 앞에 옵니다.</summary>
        public IReadOnlyList<DefeatPoint> DefeatPoints { get; }

        public SimulationReport(string policyName, int seed, IEnumerable<GameRecord> records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));

            PolicyName = policyName ?? throw new ArgumentNullException(nameof(policyName));
            Seed = seed;
            Records = new List<GameRecord>(records).AsReadOnly();

            Victories = Records.Count(r => r.Outcome == GameOutcome.Victory);
            Defeats = Records.Count(r => r.Outcome == GameOutcome.Defeat);
            Stalls = Records.Count(r => r.Outcome == GameOutcome.Stalled);

            DefeatPoints = Records
                .Where(r => r.Outcome == GameOutcome.Defeat)
                .GroupBy(r => (Chapter: r.ChapterIndex, Stage: r.StageIndex, Enemy: r.EnemyId ?? "?"))
                .Select(g => new DefeatPoint(g.Key.Chapter, g.Key.Stage, g.Key.Enemy, g.Count()))
                .OrderByDescending(p => p.Count)
                .ThenBy(p => p.ChapterIndex)
                .ThenBy(p => p.StageIndex)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>클리어율 (0~1). 기록이 없으면 0입니다.</summary>
        public double WinRate => Games == 0 ? 0 : (double)Victories / Games;

        public double AverageStagesCleared => Average(r => r.StagesCleared);
        public double AverageActions => Average(r => r.Actions);
        public double AverageFinalMaxHp => Average(r => r.FinalMaxHp);
        public double AverageFinalAttack => Average(r => r.FinalAttack);

        private double Average(Func<GameRecord, int> selector)
        {
            return Games == 0 ? 0 : Records.Average(selector);
        }

        /// <summary>
        /// 사람이 읽기 좋은 글로 정리합니다. (숫자 표기는 문화권과 무관하게 같은 형식)
        /// </summary>
        public string ToText(int maxDefeatLines = 10)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== 밸런스 시뮬레이션 결과 ===");
            sb.AppendLine($"스킬 사용 전략: {PolicyName}");
            sb.AppendLine($"판 수: {Games} (시드 {Seed})");
            sb.AppendLine($"클리어율: {Percent(WinRate)} ({Victories}/{Games})");
            sb.AppendLine($"패배: {Defeats}판, 시간 초과: {Stalls}판");
            sb.AppendLine($"평균 클리어 단계: {Number(AverageStagesCleared)}, 평균 행동 수: {Number(AverageActions)}");
            sb.AppendLine($"평균 최종 최대 체력: {Number(AverageFinalMaxHp)}, 평균 최종 공격력: {Number(AverageFinalAttack)}");

            if (DefeatPoints.Count > 0)
            {
                sb.AppendLine("패배 지점 (많은 순):");

                foreach (DefeatPoint point in DefeatPoints.Take(maxDefeatLines))
                {
                    string share = Percent((double)point.Count / Defeats);
                    sb.AppendLine($"  챕터 {point.ChapterIndex + 1} · 단계 {point.StageIndex + 1} · {point.EnemyId}: {point.Count}판 ({share})");
                }

                if (DefeatPoints.Count > maxDefeatLines)
                {
                    sb.AppendLine($"  ... 외 {DefeatPoints.Count - maxDefeatLines}곳");
                }
            }

            return sb.ToString().TrimEnd();
        }

        internal static string Percent(double ratio)
        {
            return (ratio * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
        }

        internal static string Number(double value)
        {
            return value.ToString("F1", CultureInfo.InvariantCulture);
        }
    }
}