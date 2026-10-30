#nullable enable

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 자동으로 진행한 한 판의 결과 기록입니다. (순수 데이터)
    /// </summary>
    public sealed class GameRecord
    {
        public GameOutcome Outcome { get; }

        /// <summary>끝난 시점의 챕터 번호 (0부터)</summary>
        public int ChapterIndex { get; }

        /// <summary>끝난 시점의 단계 번호 (0부터)</summary>
        public int StageIndex { get; }

        /// <summary>패배/중단 당시 맞서던 적의 Id. 승리하면 null입니다.</summary>
        public string? EnemyId { get; }

        /// <summary>마친 단계 수 (스토리 단계 포함)</summary>
        public int StagesCleared { get; }

        /// <summary>전투에서 스킬을 사용한 횟수</summary>
        public int Actions { get; }

        /// <summary>끝난 시점의 플레이어 최대 체력</summary>
        public int FinalMaxHp { get; }

        /// <summary>끝난 시점의 플레이어 공격력</summary>
        public int FinalAttack { get; }

        public GameRecord(
            GameOutcome outcome, int chapterIndex, int stageIndex, string? enemyId,
            int stagesCleared, int actions, int finalMaxHp, int finalAttack)
        {
            Outcome = outcome;
            ChapterIndex = chapterIndex;
            StageIndex = stageIndex;
            EnemyId = enemyId;
            StagesCleared = stagesCleared;
            Actions = actions;
            FinalMaxHp = finalMaxHp;
            FinalAttack = finalAttack;
        }
    }
}