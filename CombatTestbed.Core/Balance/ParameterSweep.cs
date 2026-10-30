#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 파라미터 하나(예: 플레이어 공격력)를 여러 값으로 바꿔 가며 시뮬레이션을 돌리는 도구입니다.
    /// 모든 값에 같은 시드를 써서, 결과 차이가 "난수 운"이 아니라 "값의 차이"에서 나오게 합니다.
    /// </summary>
    public static class ParameterSweep
    {
        /// <param name="parameterName">표에 표시할 파라미터 이름 (예: "플레이어 공격력")</param>
        /// <param name="values">시험할 값들 (위에서부터 차례로)</param>
        /// <param name="factoryForValue">값 하나를 받아, 그 값이 적용된 캠페인 팩토리를 돌려주는 함수</param>
        public static SweepResult Run(
            string parameterName,
            IEnumerable<int> values,
            Func<int, Func<IRandomSource, CampaignManager>> factoryForValue,
            ISkillPolicy policy,
            int gamesPerValue,
            int seed)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("파라미터 이름은 비어 있을 수 없습니다.", nameof(parameterName));
            }

            if (values == null) throw new ArgumentNullException(nameof(values));
            if (factoryForValue == null) throw new ArgumentNullException(nameof(factoryForValue));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            var valueList = new List<int>(values);

            if (valueList.Count == 0)
            {
                throw new ArgumentException("스윕할 값이 최소 1개 필요합니다.", nameof(values));
            }

            var points = new List<SweepPoint>(valueList.Count);

            foreach (int value in valueList)
            {
                var simulator = new CampaignSimulator(factoryForValue(value), policy);
                points.Add(new SweepPoint(value, simulator.Run(gamesPerValue, seed)));
            }

            return new SweepResult(parameterName, policy.Name, gamesPerValue, seed, points);
        }
    }
}