namespace CombatTestbed.Core.Domain.Actors
{
    /// <summary>
    /// 전투 액터가 플레이어 측인지, 적군 측인지 구분하는 열거형입니다.
    /// </summary>
    public enum ActorRole
    {
        Player,     // 플레이어 진영
        Enemy       // 적(일반 몬스터, 보스) 진영
    }
}