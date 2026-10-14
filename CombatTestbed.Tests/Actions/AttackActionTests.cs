using System;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Actions
{
    [TestFixture]
    public class AttackActionTests
    {
        private BattleActor CreateTestActor(string name, int maxHp, int attack, int defense)
    => TestActorFactory.Create(name, ActorRole.Player, maxHp, attack, defense);

        [Test]
        public void 정상적인_일반공격시_방어력을_반영해_피해를_입히고_성공_결과를_반환해야_한다()
        {
            // 1. Arrange: 공격자(공격력 25), 피격자(체력 100, 방어력 10)
            // 피해 공식에 따라 실제 피해는 25 - 10 = 15가 되어야 함
            var attacker = CreateTestActor("용사", maxHp: 100, attack: 25, defense: 5);
            var defender = CreateTestActor("슬라임", maxHp: 100, attack: 10, defense: 10);
            var action = new AttackAction(attacker, defender);

            // 2. Act: 일반 공격 실행
            ActionResult result = action.Execute();

            // 3. Assert: 영수증과 체력 결과 검증
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DamageDealt, Is.EqualTo(15));
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(85));
            Assert.That(result.Source, Is.EqualTo(attacker));
            Assert.That(result.Target, Is.EqualTo(defender));
        }

        [Test]
        public void 공격자가_마비상태일_경우_CanExecute는_false이고_직접_Execute를_호출하면_예외를_던져야_한다()
        {
            var attacker = CreateTestActor("용사", maxHp: 100, attack: 25, defense: 5);
            var defender = CreateTestActor("슬라임", maxHp: 100, attack: 10, defense: 10);
            attacker.StatusEffects.ApplyEffect(attacker, new ParalysisEffect(initialStacks: 1));
            var action = new AttackAction(attacker, defender);

            Assert.That(action.CanExecute(), Is.False);
            Assert.Throws<InvalidOperationException>(() => action.Execute());
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(100)); // 체력 깎이지 않음
        }

        [Test]
        public void 공격자나_대상이_null이면_ArgumentNullException을_던져야_한다()
        {
            // Arrange
            var validActor = CreateTestActor("용사", 100, 10, 5);

            // Assert: 방금 배운 생성자 null 방어 로직 검증!
            Assert.Throws<ArgumentNullException>(() => new AttackAction(null, validActor));
            Assert.Throws<ArgumentNullException>(() => new AttackAction(validActor, null));
        }
    }
}