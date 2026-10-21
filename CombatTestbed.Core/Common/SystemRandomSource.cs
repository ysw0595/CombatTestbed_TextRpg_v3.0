using System;

namespace CombatTestbed.Core.Common
{
    /// <summary>
    /// System.Random을 사용하는 실제 난수 공급자입니다.
    /// </summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource()
        {
            _random = new Random();
        }

        /// <param name="seed">같은 시드는 같은 난수 순서를 만듭니다. (재현이 필요할 때 사용)</param>
        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "최댓값은 최솟값보다 커야 합니다.");
            }

            return _random.Next(minInclusive, maxExclusive);
        }
    }
}