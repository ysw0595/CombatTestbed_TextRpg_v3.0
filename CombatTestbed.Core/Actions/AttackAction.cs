using System;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Core.Actions
{
    /// <summary>
    /// 시전자가 대상에게 기본 일반 공격을 가하는 전투 행동 커맨드입니다.
    /// </summary>
    public class AttackAction : ICombatAction
    {
        public string Name => "일반 공격";
        public BattleActor Source { get; }
        public BattleActor Target { get; }

        public AttackAction(BattleActor source, BattleActor target)
        {
            // 공격 주체나 대상이 null이면 생성 시점에 즉시 방어
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// 시전자가 행동 가능한 상태(마비/사망 아님)이고, 대상이 아직 생존해 있는지 검사합니다.
        /// </summary>
        public bool CanExecute()
        {
            return Source.CanAct && !Target.Health.IsDead;
        }

        /// <summary>
        /// 일반 공격을 실제로 실행하고 영수증(ActionResult)을 반환합니다.
        /// </summary>
        // AttackAction.cs
        public ActionResult Execute()
        {
            // 계약 위반 방어: 검증은 ActionPipeline의 책임이므로, 직접 호출 시 규칙 위반은 예외로 알림
            if (!CanExecute())
            {
                throw new InvalidOperationException(
                    $"{Name}을(를) 실행할 수 없는 상태입니다. ActionPipeline을 통해 실행하세요.");
            }

            int attackPower = Source.Stats.GetValue(StatType.Attack);

            // 방어력 계산을 포함한 피해 적용 후, 실제로 깎인 피해량을 바로 돌려받음
            int actualDamage = Target.ApplyDamage(attackPower);

            string message = $"{Source.Name}이(가) {Target.Name}에게 일반 공격으로 {actualDamage}의 피해를 입혔습니다.";
            return ActionResult.Success(Source, Target, actualDamage, message);
        }
    }
}