using System;
using CombatTestbed.Core.Actions;
#nullable enable

namespace CombatTestbed.Core.Pipeline
{
    /// <summary>
    /// 전투 행동의 실행 진입점입니다. 실행 가능 여부 검증을 전담하고,
    /// 검증을 통과한 행동만 실행한 뒤 그 결과를 이벤트로 알립니다.
    /// </summary>
    public class ActionPipeline
    {
        /// <summary>
        /// 행동 하나의 처리가 끝날 때마다 한 번 발행됩니다. (성공/실패 모두)
        /// 행동으로 인한 상태 변화(체력, 사망 등)가 모두 반영되고 해당 이벤트들이 발행된 뒤에 발행됩니다.
        /// </summary>
        public event Action<ActionResult>? ActionResolved;

        public ActionResult Execute(ICombatAction action)
        {
            // null 방어: 파이프라인에 빈 명령서가 들어오는 것을 차단
            if (action == null) throw new ArgumentNullException(nameof(action));
   
            // 게임 규칙상 실행할 수 없는 상태(마비, 대상 사망 등)는 예외가 아닌 실패 결과로 보고
               ActionResult result = action.CanExecute()
                ? action.Execute()
                : ActionResult.Failure(
                    action.Source,
                    action.Target,
                    $"{action.Source.Name}의 {action.Name} 실행이 불가능한 상태입니다.");

            // 성공/실패 어느 경로든 여기 한 곳에서만 통보
            ActionResolved?.Invoke(result);

            return result;
        }
    }
}