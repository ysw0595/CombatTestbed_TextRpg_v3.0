using CombatTestbed.Core.Common;

namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 화면 하나에 해당하는 FSM 상태입니다. 진입하면 화면을 보여 주고, 나가면 숨깁니다.
    /// 모든 화면의 동작이 같아서 하나의 클래스를 화면 수만큼 인스턴스로 만들어 씁니다.
    /// </summary>
    internal sealed class ScreenState : IState
    {
        private readonly IScreenView _view;

        public ScreenId Id { get; }

        public ScreenState(ScreenId id, IScreenView view)
        {
            Id = id;
            _view = view;
        }

        public void Enter()
        {
            _view.Show();
        }

        public void Exit()
        {
            _view.Hide();
        }
    }
}