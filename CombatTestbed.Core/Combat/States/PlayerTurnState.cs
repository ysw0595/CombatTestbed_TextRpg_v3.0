using CombatTestbed.Core.Actions;

namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 플레이어의 행동 입력을 기다리는 상태입니다. 행동이 제출되면 플레이어 턴을 실행하고 다음 상태로 넘깁니다.
    /// </summary>
    internal sealed class PlayerTurnState : BattleStateBase
    {
        public PlayerTurnState(TurnManager owner) : base(owner) { }

        public override BattlePhase Phase => BattlePhase.PlayerTurn;

        public override void SubmitPlayerAction(ICombatAction action)
        {
            Owner.BeginRound();
            Owner.RunTurn(Owner.Player, Owner.Monster, action);

            if (Owner.IsBattleOver)
            {
                Owner.GoToBattleOver();
            }
            else
            {
                Owner.GoToEnemyTurn();
            }
        }
    }
}