namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 스탯 수정치(버프/디버프)가 기본값에 연산되는 방식을 정의합니다.
    /// </summary>
    
    public enum StatModType
    {
        Flat,       // 고정 합산 (예: 공격력 +10)
        Percent     // 백분율 가산 (예: 공격력 +20% -> 0.2f)
    }
}