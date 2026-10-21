namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 챕터를 이루는 단계의 종류입니다.
    /// </summary>
    public enum StageType
    {
        Prologue,   // 챕터 시작 스토리 (전투 없음)
        Battle,     // 일반 몬스터 전투
        Episode,    // 중간 스토리 (전투 없음, 클리어 시 스킬 획득/강화 등)
        Boss        // 챕터 보스 전투
    }
}