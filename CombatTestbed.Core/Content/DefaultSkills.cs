using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;
using System.Collections.Generic;

namespace CombatTestbed.Core.Content
{
    /// <summary>
    /// 원본 게임의 보스 스킬을 효과 데이터로 옮긴 기본 콘텐츠입니다.
    /// </summary>
    public static class DefaultSkills
    {
        // ---- 성직자 ----
        public static readonly SkillDefinition PriestReadSpell = new SkillDefinition(
            "priest_readspell", "주문 낭독", new[]
            {
                SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Paralysis, 1)
            });

        public static readonly SkillDefinition PriestCross = new SkillDefinition(
            "priest_cross", "십자가", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
            });

        public static readonly SkillDefinition PriestPray = new SkillDefinition(
            "priest_pray", "기도", new[]
            {
                SkillEffectData.FlatHeal(EffectTarget.Self, 10)
            });

        // ---- 반 헬싱 ----
        public static readonly SkillDefinition VanHelsingSilverArrow = new SkillDefinition(
            "vanhelsing_silverarrow", "은 화살", new[]
            {
                SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Paralysis, 1)
            });

        public static readonly SkillDefinition VanHelsingContinuousArrow = new SkillDefinition(
            "vanhelsing_continuousarrow", "연속 화살", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.5f),
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.5f)
            });

        public static readonly SkillDefinition VanHelsingPoisonArrow = new SkillDefinition(
            "vanhelsing_poisonarrow", "독 화살", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.8f),
                SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Poison, 1)
            });

        // ---- 웨어펜리르 ----
        public static readonly SkillDefinition WereFenrirMoonLeap = new SkillDefinition(
            "werefenrir_moonleap", "달빛 강화", new[]
            {
                SkillEffectData.StatGain(EffectTarget.Self, StatType.Attack, 0.5f)
            });

        public static readonly SkillDefinition WereFenrirScratch = new SkillDefinition(
            "werefenrir_scratch", "할퀴기", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.7f),
                SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Bleed, 10)
            });

        public static readonly SkillDefinition WereFenrirBite = new SkillDefinition(
            "werefenrir_bite", "물기", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.7f)
            });

        // ---- 흡혈귀 (표시 이름은 원본의 스킬 이름을 그대로 사용) ----
        public static readonly SkillDefinition BatFire = new SkillDefinition(
            "vampire_bat_fire", "Bat_Fire", new[]
            {
        SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
            });

        public static readonly SkillDefinition BloodExplosion = new SkillDefinition(
            "vampire_blood_explosion", "Blood_Explosion", new[]
            {
        SkillEffectData.Damage(EffectTarget.Self, StatType.MaxHp, 0.3f, DamageType.True),
        SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.4f)
            });

        public static readonly SkillDefinition Thirster = new SkillDefinition(
            "vampire_thirster", "Thirster", new[]
            {
        SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.1f),
        SkillEffectData.Heal(EffectTarget.Self, StatType.Attack, 0.08f)
            });

        public static readonly SkillDefinition OverFlow = new SkillDefinition(
            "vampire_overflow", "OverFlow", new[]
            {
        SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 1)
            });

        public static readonly SkillDefinition BloodDiamond = new SkillDefinition(
            "vampire_blood_diamond", "Blood_Diamond", new[]
            {
        SkillEffectData.ApplyStatus(EffectTarget.Self, StatusEffectType.BloodDiamond, 1)
            });

        /// <summary>
        /// 흡혈귀가 배울 수 있는 전체 스킬 (시작 스킬 Bat_Fire 포함, 이미 아는 스킬은 습득 시 자동 제외됨)
        /// </summary>
        public static readonly IReadOnlyList<SkillDefinition> VampireSkills = new[]
        {
    BatFire, BloodExplosion, Thirster, OverFlow, BloodDiamond
};
    }
}