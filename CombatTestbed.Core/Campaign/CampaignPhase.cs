namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 캠페인이 지금 무엇을 기다리고 있는지를 외부에 알리는 공개용 열거형입니다.
    /// </summary>
    public enum CampaignPhase
    {
        NotStarted,   // 아직 시작 전
        Story,        // 스토리 단계(프롤로그/에피소드): Continue() 대기
        Battle,       // 전투 중: UseSkill() 대기
        StageResult,  // 단계 결과 표시: Continue() 대기
        Victory,      // 모든 챕터 클리어
        Defeat        // 플레이어 사망
    }
}