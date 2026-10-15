using System;
using CombatTestbed.Core.Actions;

namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 전투 상태들의 공통 기반입니다. 기본 동작은 '아무것도 하지 않음 / 플레이어 입력 거부'입니다.
    /// </summary>
    internal abstract class BattleStateBase : IBattleState
    {
        protected TurnManager Owner { get; }

        protected BattleStateBase(TurnManager owner)
        {
            Owner = owner;
        }

        public abstract BattlePhase Phase { get; }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void SubmitPlayerAction(ICombatAction action)
        {
            throw new InvalidOperationException(
                $"현재 단계({Phase})에서는 플레이어 행동을 제출할 수 없습니다.");
        }
    }
}