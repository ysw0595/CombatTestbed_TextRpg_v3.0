namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 스킬을 구성하는 효과의 종류입니다.
    /// </summary>
    public enum SkillEffectKind
    {
        Damage,       // 피해
        Heal,         // 회복
        ApplyStatus,  // 상태이상 부여
        StatGain      // 스탯 영구 증가
    }
}