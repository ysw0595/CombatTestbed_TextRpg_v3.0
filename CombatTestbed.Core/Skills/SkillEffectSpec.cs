using System;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// SkillEffectData를 인스펙터에서 편집할 수 있게 평평하게 펼친 "직렬화용 데이터"입니다.
    /// SkillEffectData는 생성자가 private이라 Unity가 직접 직렬화할 수 없어서 이 클래스를 거칩니다.
    /// 종류(Kind)에 따라 쓰이는 필드가 다릅니다. (자세한 의미는 SkillEffectData의 정적 생성 메서드 설명 참고)
    /// </summary>
    [Serializable]
    public sealed class SkillEffectSpec
    {
        public SkillEffectKind Kind;
        public EffectTarget Target;
        public StatType ScalingStat;
        public float Coefficient;
        public int FlatValue;
        public DamageType DamageType;
        public StatusEffectType StatusType;
        public int Stacks;

        /// <summary>
        /// 값 검증을 거치는 정식 효과 데이터로 변환합니다. (잘못된 값이면 예외)
        /// </summary>
        public SkillEffectData ToData()
        {
            switch (Kind)
            {
                case SkillEffectKind.Damage:
                    return SkillEffectData.Damage(Target, ScalingStat, Coefficient, DamageType, FlatValue);
                case SkillEffectKind.Heal:
                    return SkillEffectData.Heal(Target, ScalingStat, Coefficient, FlatValue);
                case SkillEffectKind.ApplyStatus:
                    return SkillEffectData.ApplyStatus(Target, StatusType, Stacks);
                case SkillEffectKind.StatGain:
                    return SkillEffectData.StatGain(Target, ScalingStat, Coefficient, FlatValue);
                default:
                    throw new InvalidOperationException($"알 수 없는 효과 종류입니다: {Kind}");
            }
        }

        /// <summary>
        /// 기존 효과 데이터를 편집용 스펙으로 옮깁니다. (기본 데이터를 SO로 만들 때 사용)
        /// </summary>
        public static SkillEffectSpec From(SkillEffectData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            return new SkillEffectSpec
            {
                Kind = data.Kind,
                Target = data.Target,
                ScalingStat = data.ScalingStat,
                Coefficient = data.Coefficient,
                FlatValue = data.FlatValue,
                DamageType = data.DamageType,
                StatusType = data.StatusType,
                Stacks = data.Stacks
            };
        }
    }
}