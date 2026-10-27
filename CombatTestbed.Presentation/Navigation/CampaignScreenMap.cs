using System;
using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 캠페인 단계(CampaignPhase)에 해당하는 화면을 알려 주는 매핑입니다.
    /// </summary>
    public static class CampaignScreenMap
    {
        public static ScreenId FromPhase(CampaignPhase phase)
        {
            switch (phase)
            {
                case CampaignPhase.NotStarted:
                    return ScreenId.Title;
                case CampaignPhase.Story:
                    return ScreenId.Story;
                case CampaignPhase.Battle:
                    return ScreenId.Battle;
                case CampaignPhase.StageResult:
                    return ScreenId.StageResult;
                case CampaignPhase.Victory:
                    return ScreenId.Victory;
                case CampaignPhase.Defeat:
                    return ScreenId.Defeat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, "알 수 없는 캠페인 단계입니다.");
            }
        }
    }
}