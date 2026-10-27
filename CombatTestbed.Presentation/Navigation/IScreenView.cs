namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 화면 패널 하나가 따라야 하는 계약입니다.
    /// Unity에서는 CanvasGroup을 가진 MonoBehaviour가 구현해서 보이기/숨기기를 처리합니다.
    /// </summary>
    public interface IScreenView
    {
        /// <summary>
        /// 화면을 보여 주고 입력을 받을 수 있게 합니다.
        /// </summary>
        void Show();

        /// <summary>
        /// 화면을 숨기고 입력을 막습니다.
        /// </summary>
        void Hide();
    }
}