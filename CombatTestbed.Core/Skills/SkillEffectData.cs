using System;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 스킬을 이루는 효과 하나의 '데이터'입니다. (로직은 SkillEffectExecutor가 담당)
    /// 모든 종류가 같은 필드 묶음을 공유하는 평평한 구조라서, 이후 Unity의 ScriptableObject
    /// Inspector 필드와 1:1로 대응시키기 쉽습니다. 종류별 의미는 정적 생성 메서드의 설명을 따릅니다.
    /// </summary>
    public sealed class SkillEffectData
    {
        public SkillEffectKind Kind { get; }
        public EffectTarget Target { get; }
        public StatType ScalingStat { get; }
        public float Coefficient { get; }
        public int FlatValue { get; }
        public DamageType DamageType { get; }
        public StatusEffectType StatusType { get; }
        public int Stacks { get; }

        private SkillEffectData(
            SkillEffectKind kind, EffectTarget target, StatType scalingStat, float coefficient,
            int flatValue, DamageType damageType, StatusEffectType statusType, int stacks)
        {
            Kind = kind;
            Target = target;
            ScalingStat = scalingStat;
            Coefficient = coefficient;
            FlatValue = flatValue;
            DamageType = damageType;
            StatusType = statusType;
            Stacks = stacks;
        }

        /// <summary>
        /// 피해. 효과량 = 고정값 + (스킬 사용자의 scalingStat × 계수).
        /// </summary>
        public static SkillEffectData Damage(
            EffectTarget target, StatType scalingStat, float coefficient,
            DamageType damageType = DamageType.Normal, int flatValue = 0)
        {
            return new SkillEffectData(SkillEffectKind.Damage, target, scalingStat, coefficient,
                flatValue, damageType, StatusEffectType.None, 0);
        }

        /// <summary>
        /// 회복. 효과량 = 고정값 + (스킬 사용자의 scalingStat × 계수).
        /// </summary>
        public static SkillEffectData Heal(
            EffectTarget target, StatType scalingStat, float coefficient, int flatValue = 0)
        {
            return new SkillEffectData(SkillEffectKind.Heal, target, scalingStat, coefficient,
                flatValue, DamageType.Normal, StatusEffectType.None, 0);
        }

        /// <summary>
        /// 상태이상 부여. 종류와 스택 수를 지정합니다.
        /// </summary>
        public static SkillEffectData ApplyStatus(EffectTarget target, StatusEffectType statusType, int stacks)
        {
            if (stacks <= 0) throw new ArgumentOutOfRangeException(nameof(stacks), "스택은 1 이상이어야 합니다.");
            if (statusType == StatusEffectType.None) throw new ArgumentException("상태 이상 종류를 지정해야 합니다.", nameof(statusType));

            return new SkillEffectData(SkillEffectKind.ApplyStatus, target, StatType.Attack, 0f,
                0, DamageType.Normal, statusType, stacks);
        }

        /// <summary>
        /// 스탯 영구 증가. 증가량 = 고정값 + (받는 쪽의 현재 stat × 계수). 증가시키는 스탯이 곧 계수의 기준 스탯입니다.
        /// 최대 체력(MaxHp)을 올리면 현재 체력도 같은 양만큼 함께 증가합니다.
        /// </summary>
        public static SkillEffectData StatGain(
            EffectTarget target, StatType stat, float coefficient, int flatValue = 0)
        {
            return new SkillEffectData(SkillEffectKind.StatGain, target, stat, coefficient,
                flatValue, DamageType.Normal, StatusEffectType.None, 0);
        }

        /// <summary>
        /// 고정량 회복. 기준 스탯 없이 amount만큼 회복합니다. (보상, 포션 등에 사용)
        /// </summary>
        public static SkillEffectData FlatHeal(EffectTarget target, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "양은 1 이상이어야 합니다.");

            return Heal(target, StatType.Attack, 0f, amount);
        }

        /// <summary>
        /// 고정량 스탯 영구 증가. 기준 스탯 없이 amount만큼 올립니다. (보상 등에 사용)
        /// </summary>
        public static SkillEffectData FlatStatGain(EffectTarget target, StatType stat, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "양은 1 이상이어야 합니다.");

            return StatGain(target, stat, 0f, amount);
        }
    }
}