#nullable enable
using System;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 혈석 효과: 턴 종료 시 상대에게 스택만큼 방어 무시 피해를 주고, 스택이 1 늘어납니다.
    /// 이미 걸려 있을 때 다시 부여하면 스택이 쌓여 피해가 더 강해집니다. (원본 BloodDiamondAura)
    /// 전투가 끝나면 상태이상 일괄 정리로 사라집니다.
    /// </summary>
    public class BloodDiamondEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.BloodDiamond;
        public string Id => "blood_diamond";
        public string Name => "혈석";

        public int Stacks { get; private set; }
        public bool IsExpired => Stacks <= 0;

        public BloodDiamondEffect(int initialStacks)
        {
            Stacks = Math.Max(0, initialStacks);
        }

        /// <summary>
        /// 스킬을 다시 사용했을 때 스택을 더해 더 날카로워집니다.
        /// </summary>
        public void OnApply(BattleActor target, int stacks)
        {
            if (stacks > 0)
            {
                Stacks += stacks;
            }
        }

        public void OnTurnStart(TurnContext context)
        {
            // 혈석은 턴 시작 시점에는 아무 일도 하지 않습니다.
        }

        /// <summary>
        /// 턴 종료 시: 상대가 있고 살아 있으면 스택만큼 방어 무시 피해를 주고, 스택을 1 늘립니다.
        /// </summary>
        public void OnTurnEnd(TurnContext context)
        {
            if (IsExpired) return;

            BattleActor? opponent = context.Opponent;
            if (opponent == null || opponent.Health.IsDead) return;

            opponent.ApplyTrueDamage(Stacks);
            Stacks++;
        }

        public void OnRemove(BattleActor target)
        {
            Stacks = 0;
        }
    }
}