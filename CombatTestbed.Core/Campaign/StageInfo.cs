namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 지금 진행 중인 단계가 어디인지 알려 주는 정보입니다. (화면이 챕터 제목, 스토리 선택에 사용)
    /// </summary>
    public readonly struct StageInfo
    {
        /// <summary>챕터 번호 (0부터)</summary>
        public int ChapterIndex { get; }

        /// <summary>챕터 안의 단계 번호 (0부터)</summary>
        public int StageIndex { get; }

        public ChapterDefinition Chapter { get; }
        public StageDefinition Stage { get; }

        public StageInfo(int chapterIndex, int stageIndex, ChapterDefinition chapter, StageDefinition stage)
        {
            ChapterIndex = chapterIndex;
            StageIndex = stageIndex;
            Chapter = chapter;
            Stage = stage;
        }
    }
}