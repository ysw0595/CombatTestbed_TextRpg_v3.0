using System;
using System.Collections.Generic;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 지정한 스킬 목록을 순서대로 반복해서 사용하는 선택기입니다. (원본 보스의 스킬 순환 패턴)
    /// 전투 상태는 바꾸지 않고, 선택기 자신의 다음 순번만 갱신합니다.
    /// </summary>
    public class CyclingSkillSelector : IActionSelector
    {
        private readonly List<SkillDefinition> _skills;
        private int _nextIndex;

        /// <param name="skills">순환할 스킬 목록 (1개 이상)</param>
        /// <param name="startIndex">처음 사용할 스킬의 순번. 기본은 첫 번째 스킬입니다.</param>
        public CyclingSkillSelector(IEnumerable<SkillDefinition> skills, int startIndex = 0)
        {
            if (skills == null) throw new ArgumentNullException(nameof(skills));

            var copy = new List<SkillDefinition>(skills);

            if (copy.Count == 0)
            {
                throw new ArgumentException("순환할 스킬이 최소 1개 필요합니다.", nameof(skills));
            }

            if (copy.Contains(null))
            {
                throw new ArgumentException("스킬 목록에 null이 있을 수 없습니다.", nameof(skills));
            }

            if (startIndex < 0 || startIndex >= copy.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "시작 순번이 스킬 목록 범위를 벗어났습니다.");
            }

            _skills = copy;
            _nextIndex = startIndex;
        }

        public ICombatAction SelectAction(BattleActor self, BattleActor opponent)
        {
            SkillDefinition skill = _skills[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _skills.Count;

            return new SkillAction(skill, self, opponent);
        }
    }
}