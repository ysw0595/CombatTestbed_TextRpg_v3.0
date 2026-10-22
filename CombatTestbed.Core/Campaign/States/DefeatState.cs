namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 플레이어가 사망한 종료 상태입니다. 이후 어떤 입력도 받지 않습니다.
    /// </summary>
    internal sealed class DefeatState : CampaignStateBase
    {
        public DefeatState(CampaignManager owner) : base(owner) { }

        public override CampaignPhase Phase => CampaignPhase.Defeat;

        public override void Enter()
        {
            Owner.NotifyCampaignEnded(CampaignResult.Defeat);
        }
    }
}