namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 자동으로 진행한 한 판의 결말입니다.
    /// </summary>
    public enum GameOutcome
    {
        Victory,  // 모든 챕터 클리어
        Defeat,   // 플레이어 사망
        Stalled   // 행동 횟수 상한을 넘겨서 중단 (끝나지 않는 판을 막는 안전장치)
    }
}