using System;
using System.Collections.Generic;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 스킬 하나의 정의: 식별자, 표시 이름, 효과 목록입니다. (순수 데이터)
    /// </summary>
    public sealed class SkillDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<SkillEffectData> Effects { get; }

        public SkillDefinition(string id, string name, IEnumerable<SkillEffectData> effects)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("스킬 Id는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("스킬 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            if (effects == null) throw new ArgumentNullException(nameof(effects));

            // 방어적 복사: 호출자가 나중에 원본 목록을 바꿔도 정의가 변하지 않게 함
            var copy = new List<SkillEffectData>(effects);

            if (copy.Count == 0)
            {
                throw new ArgumentException("스킬에는 최소 1개의 효과가 필요합니다.", nameof(effects));
            }

            if (copy.Contains(null))
            {
                throw new ArgumentException("효과 목록에 null이 있을 수 없습니다.", nameof(effects));
            }

            Id = id;
            Name = name;
            Effects = copy.AsReadOnly();
        }
    }
}