namespace CombatTestbed.Presentation.Battle
{
    /// <summary>
    /// 전투 화면의 View가 따라야 하는 계약입니다. (Unity에서는 uGUI를 가진 MonoBehaviour가 구현)
    /// </summary>
    public interface IBattleView
    {
        /// <summary>
        /// 화면 위쪽의 제목 줄 (예: "챕터 1 : 폭풍전야 (stage 2) - 라운드 3")
        /// </summary>
        void SetHeader(string text);

        /// <summary>
        /// 액터 한 명의 상태를 갱신합니다.
        /// </summary>
        void ShowActor(ActorSide side, ActorViewModel actor);

        /// <summary>
        /// 전투 로그에 한 줄을 추가합니다.
        /// </summary>
        void AppendLog(string line);

        /// <summary>
        /// 전투 로그를 모두 지웁니다. (새 전투 시작 시)
        /// </summary>
        void ClearLog();
    }
}