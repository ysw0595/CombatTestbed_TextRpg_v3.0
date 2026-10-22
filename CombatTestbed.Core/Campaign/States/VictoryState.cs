namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 모든 챕터를 클리어한 종료 상태입니다. 이후 어떤 입력도 받지 않습니다.
    /// </summary>
    internal sealed class VictoryState : CampaignStateBase
    {
        public VictoryState(CampaignManager owner) : base(owner) { }

        public override CampaignPhase Phase => CampaignPhase.Victory;

        public override void Enter()
        {
            Owner.NotifyCampaignEnded(CampaignResult.Victory);
        }
    }
}