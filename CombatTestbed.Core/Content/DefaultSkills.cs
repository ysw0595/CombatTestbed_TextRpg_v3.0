using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;

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

        // ---- 베어펜리르 ----
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
    }
}