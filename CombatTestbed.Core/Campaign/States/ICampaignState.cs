using CombatTestbed.Core.Common;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 캠페인 상태가 따라야 하는 계약입니다. (외부에는 공개하지 않는 내부 타입)
    /// </summary>
    internal interface ICampaignState : IState
    {
        CampaignPhase Phase { get; }

        /// <summary>
        /// "다음" 입력. 허용되지 않는 단계에서는 InvalidOperationException을 던집니다.
        /// </summary>
        void Continue();

        /// <summary>
        /// 스킬 사용 입력. 허용되지 않는 단계에서는 InvalidOperationException을 던집니다.
        /// </summary>
        void UseSkill(SkillDefinition skill);
    }
}