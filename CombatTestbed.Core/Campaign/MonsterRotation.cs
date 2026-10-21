using System;
using System.Collections.Generic;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Definitions;

namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 몬스터 풀을 순서대로 순환하며 다음 몬스터를 알려 주는 도구입니다.
    /// 시작 위치만 무작위이고, 이후에는 풀의 순서를 따라 돕니다. (원본 ranMob 방식)
    /// </summary>
    public sealed class MonsterRotation
    {
        private readonly List<ActorDefinition> _pool;
        private int _nextIndex;

        public MonsterRotation(IEnumerable<ActorDefinition> pool, IRandomSource random)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var copy = new List<ActorDefinition>(pool);

            if (copy.Count == 0)
            {
                throw new ArgumentException("몬스터 풀에 최소 1개의 몬스터가 필요합니다.", nameof(pool));
            }

            if (copy.Contains(null))
            {
                throw new ArgumentException("몬스터 풀에 null이 있을 수 없습니다.", nameof(pool));
            }

            _pool = copy;
            _nextIndex = random.Next(0, _pool.Count);
        }

        /// <summary>
        /// 다음 몬스터 정의를 반환하고, 순번을 하나 넘깁니다. (마지막 다음에는 처음으로 돌아감)
        /// </summary>
        public ActorDefinition Next()
        {
            ActorDefinition monster = _pool[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _pool.Count;
            return monster;
        }
    }
}