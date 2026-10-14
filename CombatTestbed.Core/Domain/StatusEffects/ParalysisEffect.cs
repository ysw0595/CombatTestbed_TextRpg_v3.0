using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    public class ParalysisEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Paralysis;
        public string Id => "paralysis";
        public string Name => "마비";
        public int Stacks { get; private set; }
        public bool IsExpired => Stacks <= 0;

        public ParalysisEffect(int initialStacks)
        {
            Stacks = initialStacks;
        }

        public void OnApply(BattleActor target, int additionalStacks)
        {
            Stacks += additionalStacks;
        }

        public void OnTurnStart(TurnContext context)
        {
            // 턴 시작 시점에는 행동 차단을 위해 스택을 그대로 유지합니다.
        }

        public void OnTurnEnd(TurnContext context)
        {
            if (IsExpired) return;

            // 해당 턴의 행동을 봉쇄했으므로 턴 종료 시 1스택을 소모합니다.
            Stacks--;
        }

        public void OnRemove(BattleActor target)
        {
        }
    }
}