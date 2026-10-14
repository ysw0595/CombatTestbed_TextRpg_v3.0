using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;
using NUnit.Framework;
using System;

namespace CombatTestbed.Tests.Combat
{
    [TestFixture]
    public class TurnManagerTests
    {
        private ActionPipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _pipeline = new ActionPipeline();
        }

        private BattleActor CreateTestActor(string name, ActorRole role, int maxHp, int attack, int defense)
    => TestActorFactory.Create(name, role, maxHp, attack, defense);

        [Test]
        public void 정상적인_1라운드_진행시_양측이_행동하고_라운드_차수가_증가해야_한다()
        {
            // 1. Arrange
            // 플레이어: 체력 100, 공 20, 방 5
            // 몬스터:   체력 100, 공 15, 방 5
            var player = CreateTestActor("용사", ActorRole.Player, 100, 20, 5);
            var monster = CreateTestActor("고블린", ActorRole.Enemy, 100, 15, 5);
            var turnManager = new TurnManager(player, monster, _pipeline);

            var playerAction = new AttackAction(player, monster);

            // 2. Act: 1라운드 실행
            turnManager.ExecuteRound(playerAction);

            // 3. Assert
            // 플레이어 공격 -> 몬스터 실피해: 20 - 5 = 15 (몬스터 체력: 85)
            // 몬스터 반격   -> 플레이어 실피해: 15 - 5 = 10 (플레이어 체력: 90)
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(85));
            Assert.That(player.Health.CurrentHp, Is.EqualTo(90));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
            Assert.That(turnManager.IsBattleOver, Is.False);
            Assert.That(turnManager.Winner, Is.Null);
        }

        [Test]
        public void 플레이어의_공격으로_몬스터가_사망하면_몬스터는_반격하지_못하고_전투가_종료되어야_한다()
        {
            // 1. Arrange: 몬스터 체력이 10으로, 플레이어의 공격력 20에 즉사하는 상황
            var player = CreateTestActor("용사", ActorRole.Player, 100, 20, 0);
            var monster = CreateTestActor("약한 슬라임", ActorRole.Enemy, 10, 5, 0);
            var turnManager = new TurnManager(player, monster, _pipeline);

            var playerAction = new AttackAction(player, monster);

            // 2. Act
            turnManager.ExecuteRound(playerAction);

            // 3. Assert
            Assert.That(monster.Health.IsDead, Is.True);
            Assert.That(player.Health.CurrentHp, Is.EqualTo(100)); // 몬스터가 반격하지 못했으므로 체력 보존
            Assert.That(turnManager.IsBattleOver, Is.True);
            Assert.That(turnManager.Winner, Is.EqualTo(player));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(1));  // 중도 종료 시 라운드 증가 없이 유지
        }

        [Test]
        public void 이미_전투가_종료된_상태에서_라운드_진행을_호출하면_InvalidOperationException을_던져야_한다()
        {
            // 1. Arrange: 몬스터 처치로 전투 종료 상태 유도
            var player = CreateTestActor("용사", ActorRole.Player, 100, 20, 0);
            var monster = CreateTestActor("슬라임", ActorRole.Enemy, 10, 5, 0);
            var turnManager = new TurnManager(player, monster, _pipeline);

            turnManager.ExecuteRound(new AttackAction(player, monster));
            Assert.That(turnManager.IsBattleOver, Is.True);

            // 2. Act & Assert: 종료 후 추가 진행 시도 방어
            Assert.Throws<InvalidOperationException>(() =>
            {
                turnManager.ExecuteRound(new AttackAction(player, monster));
            });
        }

        [Test]
        public void 라운드_진행에_null_액션을_전달하면_ArgumentNullException을_던져야_한다()
        {
            var player = CreateTestActor("용사", ActorRole.Player, 100, 10, 5);
            var monster = CreateTestActor("슬라임", ActorRole.Enemy, 100, 10, 5);
            var turnManager = new TurnManager(player, monster, _pipeline);

            Assert.Throws<ArgumentNullException>(() => turnManager.ExecuteRound(null));
        }

        [Test]
        public void 몬스터가_출혈이면_몬스터_턴_종료때_스택만큼_피해를_입고_스택은_유지되어야_한다()
        {
            var player = CreateTestActor("용사", ActorRole.Player, 100, 20, 0);
            var monster = CreateTestActor("고블린", ActorRole.Enemy, 100, 10, 0);
            monster.StatusEffects.ApplyEffect(monster, new BleedEffect(3));
            var turnManager = new TurnManager(player, monster, _pipeline);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            // 몬스터: 20(공격) + 3(출혈 틱) = 23 피해 -> 77, 플레이어: 10 피해 -> 90
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(77));
            Assert.That(player.Health.CurrentHp, Is.EqualTo(90));
            Assert.That(monster.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(3));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
        }

        [Test]
        public void 플레이어가_마비면_행동이_실패하지만_턴은_진행되고_마비가_소모되어야_한다()
        {
            var player = CreateTestActor("용사", ActorRole.Player, 100, 20, 0);
            var monster = CreateTestActor("고블린", ActorRole.Enemy, 100, 10, 0);
            player.StatusEffects.ApplyEffect(player, new ParalysisEffect(1));
            var turnManager = new TurnManager(player, monster, _pipeline);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(monster.Health.CurrentHp, Is.EqualTo(100)); // 플레이어 공격 불발
            Assert.That(player.Health.CurrentHp, Is.EqualTo(90));   // 몬스터는 정상 반격
            Assert.That(player.CanAct, Is.True);                    // 마비 1스택 소모 후 해제
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
        }

        [Test]
        public void 플레이어가_턴_종료_독_피해로_사망하면_몬스터는_행동하지_못하고_몬스터가_승리해야_한다()
        {
            var player = CreateTestActor("용사", ActorRole.Player, 3, 20, 0);
            var monster = CreateTestActor("고블린", ActorRole.Enemy, 100, 10, 0);
            player.StatusEffects.ApplyEffect(player, new PoisonEffect(3));
            var turnManager = new TurnManager(player, monster, _pipeline);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(player.Health.IsDead, Is.True);
            Assert.That(turnManager.Winner, Is.EqualTo(monster));
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(80)); // 플레이어 공격만 반영, 반격 없음
            Assert.That(turnManager.CurrentRound, Is.EqualTo(1));
        }
    }
}