using System;
using CombatTestbed.Core.Actions;

namespace CombatTestbed.Core.Pipeline
{
    /// <summary>
    /// 전투 행동의 실행 진입점입니다. 실행 가능 여부 검증을 전담하고,
    /// 검증을 통과한 행동만 실행합니다.
    /// </summary>
    public class ActionPipeline
    {
        public ActionResult Execute(ICombatAction action)
        {
            // null 방어: 파이프라인에 빈 명령서가 들어오는 것을 차단
            ArgumentNullException.ThrowIfNull(action);

            // 게임 규칙상 실행할 수 없는 상태(마비, 대상 사망 등)는 예외가 아닌 실패 결과로 보고
            if (!action.CanExecute())
            {
                return ActionResult.Failure(
                    action.Source,
                    action.Target,
                    $"{action.Source.Name}의 {action.Name} 실행이 불가능한 상태입니다."
                );
            }

            return action.Execute();
        }
    }
}