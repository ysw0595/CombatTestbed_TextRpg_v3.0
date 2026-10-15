namespace CombatTestbed.Core.Combat.States
{
    /// <summary>
    /// 전투가 끝난 상태입니다. 진입 시 종료를 알리고, 이후 어떤 입력도 받지 않습니다.
    /// </summary>
    internal sealed class BattleOverState : BattleStateBase
    {
        public BattleOverState(TurnManager owner) : base(owner) { }

        public override BattlePhase Phase => BattlePhase.BattleOver;

        public override void Enter()
        {
            Owner.NotifyBattleEnded();
        }
    }
}