using System;
using CombatTestbed.Core.Domain.Actors;
#nullable enable

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 턴 시작/종료 훅에 전달되는 컨텍스트입니다.
    /// 효과가 '누구의 턴인지(Owner)'와 '그 턴의 상대가 누구인지(Opponent)'를 알 수 있게 합니다.
    /// </summary>
    public readonly struct TurnContext
    {
        /// <summary>
        /// 이 턴의 주인 (효과가 걸려 있는 액터)
        /// </summary>
        public BattleActor Owner { get; }

        /// <summary>
        /// 이 턴의 상대. 상대가 없는 상황(단독 테스트 등)에서는 null입니다.
        /// </summary>
        public BattleActor? Opponent { get; }

        public TurnContext(BattleActor owner, BattleActor? opponent)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Opponent = opponent;
        }
    }
}