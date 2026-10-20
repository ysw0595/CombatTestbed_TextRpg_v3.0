#nullable enable
using System;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Core.Definitions
{
    /// <summary>
    /// 액터 정의(데이터)로부터 실제 전투에서 쓰는 객체를 만드는 팩토리입니다.
    /// </summary>
    public static class ActorFactory
    {
        /// <summary>
        /// 정의의 스탯으로 액터를 만듭니다. 체력은 가득 찬 상태로 시작합니다.
        /// </summary>
        /// <param name="instanceId">같은 정의로 여러 개를 만들 때 구분할 식별자. 생략하면 정의 Id를 씁니다.</param>
        public static BattleActor Create(ActorDefinition definition, string? instanceId = null)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var stats = new StatSheet();
            stats.SetStat(StatType.MaxHp, definition.MaxHp);
            stats.SetStat(StatType.Attack, definition.Attack);
            stats.SetStat(StatType.Defense, definition.Defense);

            return new BattleActor(instanceId ?? definition.Id, definition.Name, definition.Role, stats);
        }

        /// <summary>
        /// 정의의 행동 스킬 목록에 맞는 행동 선택기를 만듭니다.
        /// 스킬이 없으면 일반 공격만 하는 선택기, 있으면 스킬을 순환하는 선택기입니다.
        /// </summary>
        /// <param name="startIndex">스킬 순환의 시작 순번. 난수가 필요하면 호출하는 쪽이 정해서 넘깁니다.</param>
        public static IActionSelector CreateSelector(ActorDefinition definition, int startIndex = 0)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            if (definition.BehaviorSkills.Count == 0)
            {
                return new BasicAttackSelector();
            }

            return new CyclingSkillSelector(definition.BehaviorSkills, startIndex);
        }
    }
}