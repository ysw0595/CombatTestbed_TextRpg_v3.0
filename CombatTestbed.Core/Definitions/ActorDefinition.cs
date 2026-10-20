#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Definitions
{
    /// <summary>
    /// 액터(플레이어/몬스터/보스)의 기본 스탯과 행동 데이터를 담은 정의입니다. (순수 데이터)
    /// </summary>
    public sealed class ActorDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public ActorRole Role { get; }
        public int MaxHp { get; }
        public int Attack { get; }
        public int Defense { get; }

        /// <summary>
        /// 처치 시 지급하는 보상. 없으면 null입니다.
        /// </summary>
        public RewardDefinition? Reward { get; }

        /// <summary>
        /// 순환해서 사용하는 행동 스킬 목록(보스 패턴). 비어 있으면 일반 공격만 합니다.
        /// </summary>
        public IReadOnlyList<SkillDefinition> BehaviorSkills { get; }

        public ActorDefinition(
            string id, string name, ActorRole role, int maxHp, int attack, int defense = 0,
            RewardDefinition? reward = null, IEnumerable<SkillDefinition>? behaviorSkills = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("액터 Id는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("액터 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp), "최대 체력은 1 이상이어야 합니다.");
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack), "공격력은 0 이상이어야 합니다.");
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense), "방어력은 0 이상이어야 합니다.");

            // 방어적 복사: 호출자가 나중에 원본 목록을 바꿔도 정의가 변하지 않게 함
            var skills = new List<SkillDefinition>(behaviorSkills ?? new List<SkillDefinition>());

            if (skills.Contains(null!))
            {
                throw new ArgumentException("행동 스킬 목록에 null이 있을 수 없습니다.", nameof(behaviorSkills));
            }

            Id = id;
            Name = name;
            Role = role;
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            Reward = reward;
            BehaviorSkills = skills.AsReadOnly();
        }
    }
}