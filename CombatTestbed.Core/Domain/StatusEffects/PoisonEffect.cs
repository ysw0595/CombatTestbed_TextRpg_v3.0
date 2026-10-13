using System;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 중독 효과: 턴 종료 시 중첩 횟수만큼 방어 무시 피해(True Damage)를 주고, 중첩이 1 감소합니다.
    /// </summary>
    public class PoisonEffect : IStatusEffect
    {
        public StatusEffectType Type => StatusEffectType.Poison;
        public string Id => "poison";
        public string Name => "중독";

        public int Stacks { get; private set; }
        public bool IsExpired => Stacks <= 0;

        public PoisonEffect(int initialStacks)
        {
            Stacks = Math.Max(0, initialStacks);
        }

        /// <summary>
        /// 이미 중독에 걸린 상태에서 추가 중독 스킬을 맞았을 때 중첩을 합산합니다.
        /// </summary>
        public void OnApply(BattleActor target, int stacks)
        {
            if (stacks > 0)
            {
                Stacks += stacks;
            }
        }

        public void OnTurnStart(BattleActor target)
        {
            // 중독은 턴 시작 시점에는 아무 일도 하지 않으므로 비워둡니다.
        }

        /// <summary>
        /// 턴 종료 시: 중첩 수치만큼 트루 대미지를 주고 중첩을 1 깎습니다.
        /// </summary>
        public void OnTurnEnd(BattleActor target)
        {
            if (IsExpired || target == null) return;

            // 1. 현재 중첩 수치만큼 방어 무시 피해(True Damage)를 입힘
            target.ApplyTrueDamage(Stacks);

            // 2. 턴 종료 시 중첩 1 감소
            Stacks--;
        }

        public void OnRemove(BattleActor target)
        {
            // 독이 완전히 빠졌을 때 처리할 정리 작업 (스택 0 초기화)
            Stacks = 0;
        }
    }
}