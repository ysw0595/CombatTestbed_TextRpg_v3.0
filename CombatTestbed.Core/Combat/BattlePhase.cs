namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 전투가 지금 어떤 단계인지(시스템이 무엇을 기다리고 있는지)를 외부에 알리는 공개용 열거형입니다.
    /// </summary>
    public enum BattlePhase
    {
        NotStarted,   // 아직 시작 전
        PlayerTurn,   // 플레이어의 행동 입력을 기다리는 중
        EnemyTurn,    // 적 턴이 자동으로 진행되는 중
        BattleOver    // 전투 종료
    }
}