namespace CombatTestbed.Core.Domain.StatusEffects
{
    public enum StatusEffectType
    {
        None = 0,
        Poison,    // 독 (턴 종료 시 피해 & 스택 차감)
        Bleed,     // 출혈 (턴 종료 시 스택만큼 피해 & 영구 유지)
        Paralysis  // 마비 (행동 차단 & 턴 종료 시 1스택 차감)
    }
}