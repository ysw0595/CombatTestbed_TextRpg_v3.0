#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 플레이어가 배운 스킬 목록입니다. (스킬은 Id가 같으면 같은 스킬로 취급합니다.)
    /// </summary>
    public sealed class SkillBook
    {
        private readonly List<SkillDefinition> _skills = new List<SkillDefinition>();

        public SkillBook()
        {
            Skills = _skills.AsReadOnly();
        }

        /// <summary>
        /// 배운 순서대로의 스킬 목록 (읽기 전용)
        /// </summary>
        public IReadOnlyList<SkillDefinition> Skills { get; }

        /// <summary>
        /// 스킬을 새로 배웠을 때 발행됩니다. (UI의 스킬 버튼 갱신 등에 사용)
        /// </summary>
        public event Action<SkillDefinition>? SkillLearned;

        public bool Contains(SkillDefinition skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            return _skills.Exists(s => s.Id == skill.Id);
        }

        /// <returns>새로 배웠으면 true, 이미 알고 있어서 무시했으면 false</returns>
        public bool Learn(SkillDefinition skill)
        {
            if (Contains(skill)) return false;

            _skills.Add(skill);
            SkillLearned?.Invoke(skill);
            return true;
        }

        /// <summary>
        /// 풀에서 아직 배우지 않은 스킬 중 하나를 무작위로 골라 배웁니다.
        /// 배울 스킬이 하나도 남지 않았다면 아무것도 하지 않고 null을 반환합니다.
        /// </summary>
        public SkillDefinition? LearnRandom(IReadOnlyList<SkillDefinition> pool, IRandomSource random)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (random == null) throw new ArgumentNullException(nameof(random));

            // 아직 배우지 않은 후보만 추림
            var candidates = new List<SkillDefinition>();
            foreach (SkillDefinition skill in pool)
            {
                if (!Contains(skill))
                {
                    candidates.Add(skill);
                }
            }

            if (candidates.Count == 0) return null;

            SkillDefinition picked = candidates[random.Next(0, candidates.Count)];
            Learn(picked);
            return picked;
        }
    }
}