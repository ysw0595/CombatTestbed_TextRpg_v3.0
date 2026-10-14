using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;
using NUnit.Framework;
using System;

namespace CombatTestbed.Tests.Pipeline
{
    [TestFixture]
    public class ActionPipelineTests
    {
        private ActionPipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _pipeline = new ActionPipeline();
        }

        private BattleActor CreateTestActor(string name, int maxHp, int attack, int defense)
    => TestActorFactory.Create(name, ActorRole.Player, maxHp, attack, defense);

        [Test]
        public void 파이프라인에_null_액션을_전달하면_ArgumentNullException을_던져야_한다()
        {
            // Assert: null 검증
            Assert.Throws<ArgumentNullException>(() => _pipeline.Execute(null));
        }

        [Test]
        public void 유효한_일반공격_액션은_파이프라인을_통해_정상_실행되어야_한다()
        {
            // 1. Arrange: 공격자(공격력 20), 피격자(체력 100, 방어력 5) -> 실피해 15
            var attacker = CreateTestActor("용사", 100, 20, 5);
            var defender = CreateTestActor("고블린", 100, 10, 5);
            var action = new AttackAction(attacker, defender);

            // 2. Act: 파이프라인을 거쳐 실행
            ActionResult result = _pipeline.Execute(action);

            // 3. Assert: 결과 검증
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DamageDealt, Is.EqualTo(15));
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(85));
        }

        [Test]
        public void 시전자가_행동불가_상태이면_파이프라인_사전검증에서_실패_결과를_반환해야_한다()
        {
            // 1. Arrange: 마비 걸린 공격자
            var attacker = CreateTestActor("용사", 100, 20, 5);
            var defender = CreateTestActor("고블린", 100, 10, 5);
            attacker.StatusEffects.ApplyEffect(attacker, new ParalysisEffect(1));

            var action = new AttackAction(attacker, defender);

            // 2. Act: 파이프라인 실행
            ActionResult result = _pipeline.Execute(action);

            // 3. Assert: 실행되지 않고 실패 처리됨
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DamageDealt, Is.EqualTo(0));
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void 공격_피해로_인해_대상의_체력이_0_이하가_되면_사망_상태가_반영되어야_한다()
        {
            // 1. Arrange: 피격자 체력이 10, 공격력 25 -> 일격사 시나리오
            var attacker = CreateTestActor("용사", 100, 25, 0);
            var defender = CreateTestActor("약한 슬라임", 10, 5, 0);
            var action = new AttackAction(attacker, defender);

            // 2. Act: 파이프라인 실행
            ActionResult result = _pipeline.Execute(action);

            // 3. Assert: 성공 및 대상 사망 상태 검증
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(0));
            Assert.That(defender.Health.IsDead, Is.True);
        }
    }
}