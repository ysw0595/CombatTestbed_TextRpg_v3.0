using System;
using System.Collections.Generic;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Rewards
{
    /// <summary>
    /// 보상 정의를 받는 액터에게 적용하고, 효과별 결과 목록을 돌려줍니다.
    /// </summary>
    public static class RewardApplier
    {
        /// <returns>보상의 효과 순서와 같은 순서의 실행 결과 (문구 생성 등은 호출하는 쪽의 몫)</returns>
        public static IReadOnlyList<EffectOutcome> Apply(RewardDefinition reward, BattleActor recipient)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            var outcomes = new List<EffectOutcome>(reward.Effects.Count);

            foreach (SkillEffectData effect in reward.Effects)
            {
                // 보상은 받는 쪽(Self)에게만 적용되므로 상대는 필요 없음
                outcomes.Add(SkillEffectExecutor.Execute(effect, recipient, null));
            }

            return outcomes;
        }
    }
}