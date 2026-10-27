namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 게임의 화면(패널) 종류입니다.
    /// </summary>
    public enum ScreenId
    {
        Title,        // 시작 화면 (캐릭터 선택)
        Story,        // 스토리 화면 (프롤로그/에피소드)
        Battle,       // 전투 화면
        StageResult,  // 단계 결과 화면 (보상, 스킬 획득)
        Victory,      // 클리어 화면
        Defeat        // 게임 오버 화면
    }
}