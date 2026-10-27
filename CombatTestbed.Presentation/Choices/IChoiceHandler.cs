namespace CombatTestbed.Presentation.Choices
{
    /// <summary>
    /// 사용자의 선택을 전달받아 처리하는 화면 Presenter의 공통 계약입니다.
    /// (어느 화면의 선택인지는 GameController가 가려서, 해당 화면의 Presenter 하나에게만 전달합니다.)
    /// </summary>
    public interface IChoiceHandler
    {
        void HandleChoice(ActionChoice choice);
    }
}