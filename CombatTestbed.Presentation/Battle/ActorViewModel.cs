using System;
using System.Collections.Generic;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Presentation.Battle
{
    /// <summary>
    /// 액터 한 명을 화면에 그리기 위한 값 묶음입니다. (View는 이 값만 보고 그림)
    /// </summary>
    public sealed class ActorViewModel
    {
        public string Name { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int Attack { get; }

        /// <summary>
        /// 걸려 있는 상태이상의 표시 문구 (예: "중독 3")
        /// </summary>
        public IReadOnlyList<string> Statuses { get; }

        public ActorViewModel(string name, int currentHp, int maxHp, int attack, IEnumerable<string> statuses)
        {
            if (statuses == null) throw new ArgumentNullException(nameof(statuses));

            Name = name;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            Attack = attack;
            Statuses = new List<string>(statuses).AsReadOnly();
        }

        /// <summary>
        /// 체력 비율 (0~1). 체력 바의 채움 정도에 사용합니다.
        /// </summary>
        public float HpRatio => MaxHp > 0 ? Math.Min(1f, Math.Max(0f, (float)CurrentHp / MaxHp)) : 0f;

        /// <summary>
        /// 액터의 현재 상태를 읽어 화면용 값으로 만듭니다.
        /// </summary>
        public static ActorViewModel From(BattleActor actor)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));

            var statuses = new List<string>();
            foreach (IStatusEffect effect in actor.StatusEffects.Effects)
            {
                statuses.Add($"{effect.Name} {effect.Stacks}");
            }

            return new ActorViewModel(
                actor.Name,
                actor.Health.CurrentHp,
                actor.Health.MaxHp,
                actor.Stats.GetValue(StatType.Attack),
                statuses);
        }
    }
}