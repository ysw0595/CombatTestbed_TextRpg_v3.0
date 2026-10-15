using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 상대에게 방어를 무시하는 고정 피해를 주는 테스트용 가짜 행동입니다.
    /// 일반 공격(AttackAction)과 결과가 확연히 달라서, '어떤 행동이 실행되었는지' 구분하는 데 씁니다.
    /// </summary>
    public class FixedDamageActionStub : ICombatAction
    {
        private readonly int _damage;

        public string Name => "테스트용 고정 피해";
        public BattleActor Source { get; }
        public BattleActor Target { get; }

        public FixedDamageActionStub(BattleActor source, BattleActor target, int damage)
        {
            Source = source;
            Target = target;
            _damage = damage;
        }

        public bool CanExecute()
        {
            return Source.CanAct && !Target.Health.IsDead;
        }

        public ActionResult Execute()
        {
            int dealt = Target.ApplyTrueDamage(_damage);
            return ActionResult.Success(Source, Target, dealt, $"{Source.Name}의 고정 피해 {dealt}");
        }
    }
}