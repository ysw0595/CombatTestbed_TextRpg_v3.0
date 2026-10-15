namespace CombatTestbed.Core.Common
{
    /// <summary>
    /// 상태 머신이 관리하는 상태가 따라야 하는 최소 계약입니다.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 이 상태로 진입할 때 호출됩니다.
        /// </summary>
        void Enter();

        /// <summary>
        /// 이 상태에서 빠져나갈 때 호출됩니다.
        /// </summary>
        void Exit();
    }
}