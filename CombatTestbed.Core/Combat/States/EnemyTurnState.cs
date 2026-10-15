using CombatTestbed.Core.Actions;

namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 적 턴이 자동으로 진행되는 상태입니다. 진입하면 행동을 고르고 실행한 뒤 다음 상태로 넘깁니다.
    /// </summary>
    internal sealed class EnemyTurnState : BattleStateBase
    {
        public EnemyTurnState(TurnManager owner) : base(owner) { }

        public override BattlePhase Phase => BattlePhase.EnemyTurn;

        public override void Enter()
        {
            if (Owner.Monster.CanAct)
            {
                ICombatAction action = Owner.SelectMonsterAction();
                Owner.RunTurn(Owner.Monster, Owner.Player, action);
            }
            else
            {
                // 행동 불능: 행동 선택 없이 턴을 건너뜀
                Owner.SkipTurn(Owner.Monster, Owner.Player);
            }

            if (Owner.IsBattleOver)
            {
                Owner.GoToBattleOver();
            }
            else
            {
                // 다음 PlayerTurn 진입 통보 시점에 이미 새 라운드 번호가 되어 있도록 먼저 증가
                Owner.AdvanceRound();
                Owner.GoToPlayerTurn();
            }
        }
    }
}