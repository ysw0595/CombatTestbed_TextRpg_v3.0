using System;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign.States
{
    /// <summary>
    /// 전투 단계(일반/보스)입니다. 진입하면 전투를 시작하고, 스킬 입력으로 전투를 진행합니다.
    /// </summary>
    internal sealed class BattleStageState : CampaignStateBase
    {
        public BattleStageState(CampaignManager owner) : base(owner) { }

        public override CampaignPhase Phase => CampaignPhase.Battle;

        public override void Enter()
        {
            Owner.NotifyStageStarted();
            Owner.StartBattle();
        }

        public override void UseSkill(SkillDefinition skill)
        {
            if (!Owner.SkillBook.Contains(skill))
            {
                throw new ArgumentException("배우지 않은 스킬입니다.", nameof(skill));
            }

            Owner.SubmitSkill(skill);
        }
    }
}