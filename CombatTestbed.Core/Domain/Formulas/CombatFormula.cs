using System;

namespace CombatTestbed.Core.Domain.Formulas
{
    /// <summary>
    /// 전투 수치 계산 공식 모음입니다. 엔티티(BattleActor)에서 공식을 분리해 단독으로 테스트하고 조정할 수 있게 합니다.
    /// </summary>
    public static class CombatFormula
    {
        /// <summary>
        /// 효과량 = 고정값 + (기준 스탯 × 계수, 소수점 버림). 결과가 음수면 0입니다.
        /// </summary>
        public static int CalculateAmount(int flatValue, int scalingStatValue, float coefficient)
        {
            // float은 0.7처럼 2진수로 정확히 표현되지 않는 값이 있어서, 그대로 곱하면
            // 100 * 0.7f가 69.99999...가 되고 소수점을 버리면 69가 되는 오류가 생길 수 있습니다.
            // decimal로 변환하면 0.7이 정확히 0.7로 계산되어 의도한 값(70)이 나옵니다.
            int scaled = (int)(scalingStatValue * (decimal)coefficient);
            return Math.Max(0, flatValue + scaled);
        }

        /// <summary>
        /// 방어력 적용: [들어온 피해 - 방어력], 최소 1 피해를 보장합니다.
        /// </summary>
        public static int ApplyDefense(int rawDamage, int defense)
        {
            return Math.Max(1, rawDamage - defense);
        }
    }
}