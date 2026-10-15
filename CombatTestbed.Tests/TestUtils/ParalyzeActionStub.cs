using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 상대에게 마비를 부여하는 테스트용 가짜 행동입니다.
    /// </summary>
    public class ParalyzeActionStub : ICombatAction
    {
        private readonly int _stacks;

        public string Name => "테스트용 마비 부여";
        public BattleActor Source { get; }
        public BattleActor Target { get; }

        public ParalyzeActionStub(BattleActor source, BattleActor target, int stacks)
        {
            Source = source;
            Target = target;
            _stacks = stacks;
        }

        public bool CanExecute()
        {
            return Source.CanAct && !Target.Health.IsDead;
        }

        public ActionResult Execute()
        {
            Target.StatusEffects.ApplyEffect(Target, new ParalysisEffect(_stacks));
            return ActionResult.Success(Source, Target, 0, $"{Source.Name}이(가) 마비를 부여했다");
        }
    }
}