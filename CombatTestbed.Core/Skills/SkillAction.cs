using System;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 스킬 정의(효과 목록)를 순서대로 실행하는 전투 행동 커맨드입니다.
    /// </summary>
    public class SkillAction : ICombatAction
    {
        public SkillDefinition Definition { get; }
        public string Name => Definition.Name;
        public BattleActor Source { get; }
        public BattleActor Target { get; }

        public SkillAction(SkillDefinition definition, BattleActor source, BattleActor target)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// 시전자가 행동 가능하고 대상이 생존해 있는지 검사합니다.
        /// </summary>
        public bool CanExecute()
        {
            return Source.CanAct && !Target.Health.IsDead;
        }

        public ActionResult Execute()
        {
            // 계약 위반 방어: 검증은 ActionPipeline의 책임이므로 직접 호출 시 규칙 위반은 예외로 알림
            if (!CanExecute())
            {
                throw new InvalidOperationException(
                    $"{Name}을(를) 실행할 수 없는 상태입니다. ActionPipeline을 통해 실행하세요.");
            }

            // 상대에게 준 피해만 합산 (자해 등 자신에게 준 피해는 제외)
            int damageToTarget = 0;

            foreach (SkillEffectData effect in Definition.Effects)
            {
                EffectOutcome outcome = SkillEffectExecutor.Execute(effect, Source, Target);

                if (outcome.Kind == SkillEffectKind.Damage && outcome.Receiver == Target)
                {
                    damageToTarget += outcome.Amount;
                }
            }

            string message = damageToTarget > 0
                ? $"{Source.Name}이(가) {Target.Name}에게 {Name}을(를) 사용해 {damageToTarget}의 피해를 입혔습니다."
                : $"{Source.Name}이(가) {Name}을(를) 사용했습니다.";

            return ActionResult.Success(Source, Target, damageToTarget, message);
        }
    }
}