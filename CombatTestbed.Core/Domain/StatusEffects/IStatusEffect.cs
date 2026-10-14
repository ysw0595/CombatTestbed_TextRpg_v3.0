using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 모든 상태 이상(중독, 출혈, 마비, 버프/디버프 등)이 따라야 하는 공통 인터페이스입니다.
    /// </summary>
    public interface IStatusEffect
    {
        StatusEffectType Type { get; } // 상태 이상 고유 타입 (Key)
        string Id { get; }          // 상태 이상 식별자 (예: "poison", "bleed")
        string Name { get; }        // 화면 표시용 이름 (예: "중독", "출혈")
        int Stacks { get; }         // 현재 중첩 수치
        bool IsExpired { get; }     // 효과가 다 끝나서 제거되어야 하는지 여부

        /// <summary>
        /// 이미 걸려 있는 상태 이상에 추가 부여로 중첩이 더해질 때 호출됩니다.
        /// (처음 부여될 때는 호출되지 않습니다.)
        /// </summary>
        void OnApply(BattleActor target, int stacks);

        /// <summary>
        /// 액터의 턴이 시작될 때 발동할 효과. 소유자와 상대를 컨텍스트로 받습니다.
        /// </summary>
        void OnTurnStart(TurnContext context);

        /// <summary>
        /// 액터의 턴이 끝날 때 발동할 효과 (중독/출혈 틱 피해, 지속시간 감소 등).
        /// 소유자와 상대를 컨텍스트로 받습니다.
        /// </summary>
        void OnTurnEnd(TurnContext context);

        /// <summary>
        /// 상태 이상이 액터에게서 제거될 때 정리(Cleanup) 작업을 수행합니다.
        /// </summary>
        void OnRemove(BattleActor target);
    }
}