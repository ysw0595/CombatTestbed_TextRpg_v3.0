using System;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 지정한 함수로 행동을 만들어 주고, 호출 횟수와 받은 인자를 기록하는 테스트용 가짜 선택기입니다.
    /// </summary>
    public class StubActionSelector : IActionSelector
    {
        private readonly Func<BattleActor, BattleActor, ICombatAction> _factory;

        public int CallCount { get; private set; }
        public BattleActor? LastSelf { get; private set; }
        public BattleActor? LastOpponent { get; private set; }

        public StubActionSelector(Func<BattleActor, BattleActor, ICombatAction> factory)
        {
            _factory = factory;
        }

        public ICombatAction SelectAction(BattleActor self, BattleActor opponent)
        {
            CallCount++;
            LastSelf = self;
            LastOpponent = opponent;
            return _factory(self, opponent);
        }
    }
}