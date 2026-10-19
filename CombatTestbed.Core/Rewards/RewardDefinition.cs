using System;
using System.Collections.Generic;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Rewards
{
    /// <summary>
    /// 몬스터 처치 보상 하나의 정의: 식별자와 효과 목록입니다. (순수 데이터)
    /// 스킬과 같은 효과 데이터를 쓰되, 보상은 받는 쪽(Self)에게만 적용되어야 합니다.
    /// </summary>
    public sealed class RewardDefinition
    {
        public string Id { get; }
        public IReadOnlyList<SkillEffectData> Effects { get; }

        public RewardDefinition(string id, IEnumerable<SkillEffectData> effects)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("보상 Id는 비어 있을 수 없습니다.", nameof(id));
            }

            if (effects == null) throw new ArgumentNullException(nameof(effects));

            // 방어적 복사: 호출자가 나중에 원본 목록을 바꿔도 정의가 변하지 않게 함
            var copy = new List<SkillEffectData>(effects);

            if (copy.Count == 0)
            {
                throw new ArgumentException("보상에는 최소 1개의 효과가 필요합니다.", nameof(effects));
            }

            if (copy.Contains(null))
            {
                throw new ArgumentException("효과 목록에 null이 있을 수 없습니다.", nameof(effects));
            }

            foreach (SkillEffectData effect in copy)
            {
                if (effect.Target != EffectTarget.Self)
                {
                    throw new ArgumentException("보상 효과는 모두 받는 쪽(Self)을 대상으로 해야 합니다.", nameof(effects));
                }
            }

            Id = id;
            Effects = copy.AsReadOnly();
        }
    }
}