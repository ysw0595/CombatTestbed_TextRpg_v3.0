using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 턴 종료 시 '상대'에게 고정 피해를 주는 테스트용 가짜 효과입니다.
    /// 훅이 받은 컨텍스트를 기록해, 소유자와 상대가 올바르게 전달되는지 검증하는 데 씁니다.
    /// </summary>
    public class OpponentDamageEffectStub : IStatusEffect
    {
        private readonly int _damage;

        public OpponentDamageEffectStub(int damage)
        {
            _damage = damage;
        }

        // 테스트 전용: 실제 효과(독/출혈/마비)와 겹치지 않는 종류를 사용
        public StatusEffectType Type => StatusEffectType.None;
        public string Id => "test_opponent_damage";
        public string Name => "테스트용 상대 피해";
        public int Stacks => 1;
        public bool IsExpired => false;

        public TurnContext? LastTurnStartContext { get; private set; }
        public TurnContext? LastTurnEndContext { get; private set; }

        public void OnApply(BattleActor target, int stacks) { }

        public void OnTurnStart(TurnContext context)
        {
            LastTurnStartContext = context;
        }

        public void OnTurnEnd(TurnContext context)
        {
            LastTurnEndContext = context;
            context.Opponent?.ApplyTrueDamage(_damage);
        }

        public void OnRemove(BattleActor target) { }
    }
}