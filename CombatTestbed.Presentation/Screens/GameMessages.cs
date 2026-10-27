using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 전투 화면을 제외한 화면들의 문구와 선택지 식별자를 한 곳에 모은 클래스입니다.
    /// </summary>
    public static class GameMessages
    {
        public const string GameTitle = "IF THE NIGHT COMES";
        public const string TitleSubtitle = "캐릭터를 선택하세요.";

        public const string ContinueChoiceId = "continue";
        public const string ContinueLabel = "다음";
        public const string RestartChoiceId = "restart";
        public const string RestartLabel = "다시 시작";
        public const string QuitChoiceId = "quit";
        public const string QuitLabel = "종료";

        public static string StageTitle(StageInfo stage)
        {
            return $"챕터 {stage.ChapterIndex + 1} : {stage.Chapter.Name} (stage {stage.StageIndex + 1})";
        }

        public static string StageClearedTitle(StageInfo stage)
        {
            return StageTitle(stage) + " 클리어";
        }

        public static string SkillLearned(SkillDefinition skill)
        {
            return $"{skill.Name}이(가) 추가되었습니다.";
        }

        public static string EndingTitle(CampaignResult result)
        {
            return result == CampaignResult.Victory ? "게임 클리어" : "게임 오버";
        }
    }
}