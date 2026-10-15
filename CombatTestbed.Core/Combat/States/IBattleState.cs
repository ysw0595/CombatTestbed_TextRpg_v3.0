using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 전투 상태가 따라야 하는 계약입니다. (외부에는 공개하지 않는 내부 타입)
    /// </summary>
    internal interface IBattleState : IState
    {
        BattlePhase Phase { get; }

        /// <summary>
        /// 플레이어 행동을 제출합니다. 입력을 받는 상태가 아니면 InvalidOperationException을 던집니다.
        /// </summary>
        void SubmitPlayerAction(ICombatAction action);
    }
}