using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Actions
{
    public interface ICombatAction
    {
        string Name { get; }
        BattleActor Source { get; }
        BattleActor Target { get; }

        /// <summary>
        /// 행동을 실행할 수 있는 상태인지 검증합니다.
        /// </summary>
        bool CanExecute();

        // ICombatAction.cs
        /// <summary>
        /// 실제 행동을 수행하고 결과를 반환합니다.
        /// CanExecute()가 true일 때만 호출해야 하며, 그렇지 않으면 InvalidOperationException을 던집니다.
        /// 일반적으로는 ActionPipeline.Execute를 통해 호출합니다.
        /// </summary>
        ActionResult Execute();
    }
}