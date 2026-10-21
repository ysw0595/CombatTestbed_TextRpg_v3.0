using System;
using System.Collections.Generic;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 미리 정해 둔 값을 순서대로 돌려주는 테스트용 난수 공급자입니다.
    /// 요청한 범위를 벗어나는 값은 테스트 작성 실수이므로 예외로 알려 줍니다.
    /// </summary>
    public class SequenceRandomSource : IRandomSource
    {
        private readonly Queue<int> _values;

        public int CallCount { get; private set; }
        public int LastMinInclusive { get; private set; }
        public int LastMaxExclusive { get; private set; }

        public SequenceRandomSource(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            CallCount++;
            LastMinInclusive = minInclusive;
            LastMaxExclusive = maxExclusive;

            if (_values.Count == 0)
            {
                throw new InvalidOperationException("준비된 난수 값이 없습니다.");
            }

            int value = _values.Dequeue();

            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException(
                    $"준비된 값 {value}이(가) 요청 범위 [{minInclusive}, {maxExclusive})를 벗어났습니다.");
            }

            return value;
        }
    }
}