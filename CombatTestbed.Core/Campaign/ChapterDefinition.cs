#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Definitions;

namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 챕터 하나의 정의: 단계 목록, 일반 몬스터 풀, 보스입니다. (순수 데이터)
    /// </summary>
    public sealed class ChapterDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<StageDefinition> Stages { get; }

        /// <summary>
        /// 일반 전투 단계에서 순환해서 나오는 몬스터 풀
        /// </summary>
        public IReadOnlyList<ActorDefinition> MonsterPool { get; }

        /// <summary>
        /// 보스 단계의 보스. 보스 단계가 없는 챕터면 null입니다.
        /// </summary>
        public ActorDefinition? Boss { get; }

        public ChapterDefinition(
            string id, string name, IEnumerable<StageDefinition> stages,
            IEnumerable<ActorDefinition>? monsterPool = null, ActorDefinition? boss = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("챕터 Id는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("챕터 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            if (stages == null) throw new ArgumentNullException(nameof(stages));

            // 방어적 복사: 호출자가 나중에 원본 목록을 바꿔도 정의가 변하지 않게 함
            var stageList = new List<StageDefinition>(stages);
            var pool = new List<ActorDefinition>(monsterPool ?? new List<ActorDefinition>());

            if (stageList.Count == 0)
            {
                throw new ArgumentException("챕터에는 최소 1개의 단계가 필요합니다.", nameof(stages));
            }

            if (stageList.Contains(null!))
            {
                throw new ArgumentException("단계 목록에 null이 있을 수 없습니다.", nameof(stages));
            }

            if (pool.Contains(null!))
            {
                throw new ArgumentException("몬스터 풀에 null이 있을 수 없습니다.", nameof(monsterPool));
            }

            // 단계가 요구하는 데이터가 챕터에 갖춰져 있는지 만드는 순간 확인
            if (stageList.Exists(s => s.Type == StageType.Battle) && pool.Count == 0)
            {
                throw new ArgumentException("전투 단계가 있는 챕터에는 몬스터 풀이 필요합니다.", nameof(monsterPool));
            }

            if (stageList.Exists(s => s.Type == StageType.Boss) && boss == null)
            {
                throw new ArgumentException("보스 단계가 있는 챕터에는 보스가 필요합니다.", nameof(boss));
            }

            Id = id;
            Name = name;
            Stages = stageList.AsReadOnly();
            MonsterPool = pool.AsReadOnly();
            Boss = boss;
        }
    }
}