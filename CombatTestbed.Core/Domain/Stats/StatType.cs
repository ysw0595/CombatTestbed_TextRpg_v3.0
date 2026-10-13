namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 게임 내에서 액터(플레이어, 몬스터, 보스)가 가질 수 있는 스탯의 종류를 정의.
    /// </summary>

    public enum StatType
    {
        MaxHp,      // 최대 체력 상한선
        Attack,     // 공격력
        Defense     // 방어력
    }
}