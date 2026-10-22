using System;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 캠페인 상태들의 공통 기반입니다. 기본 동작은 '아무것도 하지 않음 / 모든 입력 거부'입니다.
    /// </summary>
    internal abstract class CampaignStateBase : ICampaignState
    {
        protected CampaignManager Owner { get; }

        protected CampaignStateBase(CampaignManager owner)
        {
            Owner = owner;
        }

        public abstract CampaignPhase Phase { get; }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Continue()
        {
            throw new InvalidOperationException($"현재 단계({Phase})에서는 계속 진행할 수 없습니다.");
        }

        public virtual void UseSkill(SkillDefinition skill)
        {
            throw new InvalidOperationException($"현재 단계({Phase})에서는 스킬을 사용할 수 없습니다.");
        }
    }
}