using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 스토리 문구를 공급하는 계약입니다. 문구의 출처(Unity의 텍스트 에셋 등)는 구현이 정합니다.
    /// 원본의 시나리오 txt 파일 읽기가 이 계약 뒤로 옮겨집니다.
    /// </summary>
    public interface IStoryTextProvider
    {
        /// <summary>
        /// 스토리 단계(프롤로그/에피소드)에서 보여 줄 문구
        /// </summary>
        string GetStageText(StageInfo stage);

        /// <summary>
        /// 캠페인이 끝났을 때 보여 줄 문구
        /// </summary>
        /// <param name="lastStage">마지막으로 진행한 단계 (패배한 보스 등에 따라 다른 문구를 고를 때 사용)</param>
        string GetEndingText(CampaignResult result, StageInfo lastStage);
    }
}