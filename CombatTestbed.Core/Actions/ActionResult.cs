using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Actions
{
    /// <summary>
    /// 전투 행동(공격, 스킬 등)의 실행 결과를 담아 반환하는 데이터 모델입니다.
    /// </summary>
    public class ActionResult
    {
        public bool IsSuccess { get; }
        public BattleActor Source { get; }
        public BattleActor Target { get; }
        public int DamageDealt { get; }
        public string Message { get; }

        public ActionResult(bool isSuccess, BattleActor source, BattleActor target, int damageDealt, string message)
        {
            IsSuccess = isSuccess;
            Source = source;
            Target = target;
            DamageDealt = damageDealt;
            Message = message;
        }

        // 실패 결과를 편하게 생성하기 위한 정적 메서드
        public static ActionResult Failure(BattleActor source, BattleActor target, string reason)
        {
            return new ActionResult(false, source, target, 0, reason);
        }

        // 성공 결과를 편하게 생성하기 위한 정적 메서드
        public static ActionResult Success(BattleActor source, BattleActor target, int damageDealt, string message)
        {
            return new ActionResult(true, source, target, damageDealt, message);
        }
    }
}