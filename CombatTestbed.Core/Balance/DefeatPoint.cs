namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// "어느 단계에서 어떤 적에게 몇 판을 졌는가"를 모은 집계 한 줄입니다.
    /// </summary>
    public sealed class DefeatPoint
    {
        public int ChapterIndex { get; }
        public int StageIndex { get; }
        public string EnemyId { get; }
        public int Count { get; }

        public DefeatPoint(int chapterIndex, int stageIndex, string enemyId, int count)
        {
            ChapterIndex = chapterIndex;
            StageIndex = stageIndex;
            EnemyId = enemyId;
            Count = count;
        }
    }
}