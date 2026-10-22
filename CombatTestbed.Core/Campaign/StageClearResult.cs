#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 단계를 마친 결과입니다. 화면은 이 결과로 "~를 얻었다" 같은 문구를 만듭니다.
    /// </summary>
    public sealed class StageClearResult
    {
        public StageInfo Stage { get; }

        /// <summary>
        /// 이 단계에서 받은 보상의 적용 결과 (몬스터 처치 보상 -> 단계 클리어 보상 순서)
        /// </summary>
        public IReadOnlyList<EffectOutcome> Rewards { get; }

        /// <summary>
        /// 이 단계에서 새로 배운 스킬. 없으면 null입니다.
        /// </summary>
        public SkillDefinition? LearnedSkill { get; }

        public StageClearResult(StageInfo stage, IEnumerable<EffectOutcome> rewards, SkillDefinition? learnedSkill)
        {
            if (rewards == null) throw new ArgumentNullException(nameof(rewards));

            Stage = stage;
            Rewards = new List<EffectOutcome>(rewards).AsReadOnly();
            LearnedSkill = learnedSkill;
        }
    }
}