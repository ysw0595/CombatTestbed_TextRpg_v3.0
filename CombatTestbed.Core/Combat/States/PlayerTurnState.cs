using CombatTestbed.Core.Actions;

namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 플레이어의 행동 입력을 기다리는 상태입니다. 행동이 제출되면 플레이어 턴을 실행하고 다음 상태로 넘깁니다.
    /// 플레이어가 행동할 수 없는 상태(마비 등)라면 입력을 기다리지 않고 턴을 자동으로 넘깁니다.
    /// </summary>
    internal sealed class PlayerTurnState : BattleStateBase
    {
        public PlayerTurnState(TurnManager owner) : base(owner) { }

        public override BattlePhase Phase => BattlePhase.PlayerTurn;

        public override void Enter()
        {
            // 입력을 받을 수 있는 상태면 그대로 입력 대기
            if (Owner.Player.CanAct) return;

            // 행동 불능: 입력 없이 이번 라운드의 플레이어 턴을 건너뜀
            Owner.BeginRound();
            Owner.SkipTurn(Owner.Player, Owner.Monster);
            MoveToNextState();
        }

        public override void SubmitPlayerAction(ICombatAction action)
        {
            Owner.BeginRound();
            Owner.RunTurn(Owner.Player, Owner.Monster, action);
            MoveToNextState();
        }

        private void MoveToNextState()
        {
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