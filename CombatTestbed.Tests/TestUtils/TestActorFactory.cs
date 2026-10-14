using System;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 테스트에서 BattleActor를 간편하게 만들기 위한 공용 팩토리입니다.
    /// 지정하지 않은 값은 기본값을 사용하므로, 각 테스트는 '그 테스트에 의미 있는 값'만 적으면 됩니다.
    /// </summary>
    public static class TestActorFactory
    {
        public static BattleActor Create(
            string name = "테스트 액터",
            ActorRole role = ActorRole.Player,
            int maxHp = 100,
            int attack = 10,
            int defense = 0)
        {
            var stats = new StatSheet();
            stats.SetStat(StatType.MaxHp, maxHp);
            stats.SetStat(StatType.Attack, attack);
            stats.SetStat(StatType.Defense, defense);

            return new BattleActor(Guid.NewGuid().ToString(), name, role, stats);
        }
    }
}