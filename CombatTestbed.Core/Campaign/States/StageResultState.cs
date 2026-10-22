namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 단계 결과를 보여 주는 상태입니다. 진입 시 결과를 알리고, "다음" 입력으로 다음 단계로 넘어갑니다.
    /// </summary>
    internal sealed class StageResultState : CampaignStateBase
    {
        public StageResultState(CampaignManager owner) : base(owner) { }

        public override CampaignPhase Phase => CampaignPhase.StageResult;

        public override void Enter()
        {
            Owner.NotifyStageCleared();
        }

        public override void Continue()
        {
            Owner.AdvanceToNextStage();
        }
    }
}