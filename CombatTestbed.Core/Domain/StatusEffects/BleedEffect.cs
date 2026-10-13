using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 출혈 효과: 턴 종료 시 중첩 수치만큼 방어 무시 피해(True Damage)를 줍니다.
    /// 중독과 달리 턴이 지나도 중첩이 자동으로 감소하지 않고,
    /// 정화(RemoveEffect)되기 전까지 유지됩니다.
    /// </summary>
    public class BleedEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Bleed;
        public string Id => "bleed";
        public string Name => "출혈";

        public int Stacks { get; private set; }

        // 스스로 감소하지 않으므로 외부 정화로 스택이 0이 될 때만 만료됩니다.
        public bool IsExpired => Stacks <= 0;

        public BleedEffect(int initialStacks)
        {
            Stacks = initialStacks;
        }

        /// <summary>
        /// 추가 출혈 공격을 맞았을 때 중첩을 누적합니다.
        /// </summary>
        public void OnApply(BattleActor target, int additionalStacks)
        {
            Stacks += additionalStacks;
        }

        public void OnTurnStart(BattleActor target)
        {
            // 출혈은 턴 시작 시점에는 아무 일도 하지 않습니다.
        }

        /// <summary>
        /// 턴 종료 시: 현재 중첩 수치만큼 방어 무시 피해를 입히고, 중첩은 유지합니다.
        /// </summary>
        public void OnTurnEnd(BattleActor target)
        {
            if (IsExpired || target == null) return;

            target.ApplyTrueDamage(Stacks);
        }

        public void OnRemove(BattleActor target)
        {
            // 정화로 제거될 때 스택을 0으로 정리합니다.
            Stacks = 0;
        }
    }
}