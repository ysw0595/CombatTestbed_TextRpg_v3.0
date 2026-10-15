using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 액터가 이번 턴에 어떤 행동을 할지 '고르는' 전략입니다.
    /// 선택만 하고 실행은 하지 않으며(실행은 ActionPipeline의 책임), 전투 상태를 변경해서도 안 됩니다.
    /// </summary>
    public interface IActionSelector
    {
        /// <param name="self">행동하는 액터</param>
        /// <param name="opponent">상대 액터</param>
        /// <returns>이번 턴에 실행할 행동 (null이면 안 됨)</returns>
        ICombatAction SelectAction(BattleActor self, BattleActor opponent);
    }
}