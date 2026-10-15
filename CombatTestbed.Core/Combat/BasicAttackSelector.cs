using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 항상 상대에게 일반 공격을 선택하는 기본 선택기입니다. (기존 TurnManager의 하드코딩된 동작과 동일)
    /// </summary>
    public class BasicAttackSelector : IActionSelector
    {
        public ICombatAction SelectAction(BattleActor self, BattleActor opponent)
        {
            return new AttackAction(self, opponent);
        }
    }
}