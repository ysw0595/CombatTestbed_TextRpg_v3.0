using System;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 스토리 단계(프롤로그/에피소드)입니다. 스토리를 보여 주는 동안 "다음" 입력을 기다립니다.
    /// </summary>
    internal sealed class StoryStageState : CampaignStateBase
    {
        public StoryStageState(CampaignManager owner) : base(owner) { }

        public override CampaignPhase Phase => CampaignPhase.Story;

        public override void Enter()
        {
            Owner.NotifyStageStarted();
        }

        public override void Continue()
        {
            // 전투가 없으므로 몬스터 보상 없이 단계를 마침 (에피소드의 스킬 획득/강화는 여기서 적용)
            Owner.CompleteStage(Array.Empty<EffectOutcome>());
        }
    }
}