using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Combat
{
    [TestFixture]
    public class ActionSelectorTests
    {
        [Test]
        public void BasicAttackSelector는_자신이_상대에게_하는_일반공격을_선택해야_한다()
        {
            var self = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy);
            var opponent = TestActorFactory.Create(name: "용사", role: ActorRole.Player);
            var selector = new BasicAttackSelector();

            ICombatAction action = selector.SelectAction(self, opponent);

            Assert.That(action, Is.TypeOf<AttackAction>());
            Assert.That(action.Source, Is.SameAs(self));
            Assert.That(action.Target, Is.SameAs(opponent));
        }

        [Test]
        public void 주입한_선택기가_고른_행동이_몬스터_턴에_실제로_실행되어야_한다()
        {
            // 플레이어 방어력 50: 일반 공격이면 최소 피해 1, 고정 피해면 7이 들어가므로 구분 가능
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20, defense: 50);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var selector = new StubActionSelector((self, opponent) => new FixedDamageActionStub(self, opponent, 7));
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(player.Health.CurrentHp, Is.EqualTo(93)); // 100 - 고정 피해 7
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(80)); // 플레이어 공격 20
        }

        [Test]
        public void 선택기는_몬스터_자신과_플레이어를_상대로_전달받아야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var selector = new StubActionSelector((self, opponent) => new AttackAction(self, opponent));
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(selector.CallCount, Is.EqualTo(1));
            Assert.That(selector.LastSelf, Is.SameAs(monster));
            Assert.That(selector.LastOpponent, Is.SameAs(player));
        }

        [Test]
        public void 플레이어_턴에서_전투가_끝나면_선택기는_호출되지_않아야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 10, attack: 10);
            var selector = new StubActionSelector((self, opponent) => new AttackAction(self, opponent));
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);

            turnManager.ExecuteRound(new AttackAction(player, monster)); // 몬스터 즉사

            Assert.That(turnManager.IsBattleOver, Is.True);
            Assert.That(selector.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void 선택기가_null을_반환하면_몬스터_턴이_시작되기_전에_예외가_발생해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var selector = new StubActionSelector((self, opponent) => null);
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);
            var startedTurns = new List<string>();
            turnManager.TurnStarted += actor => startedTurns.Add(actor.Name);

            Assert.Throws<InvalidOperationException>(
                () => turnManager.ExecuteRound(new AttackAction(player, monster)));

            Assert.That(startedTurns, Is.EqualTo(new[] { "용사" })); // 몬스터 턴은 시작되지 않음
        }

        [Test]
        public void 선택기는_라운드마다_몬스터_턴에_한번씩_호출되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var selector = new StubActionSelector((self, opponent) => new AttackAction(self, opponent));
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);

            turnManager.ExecuteRound(new AttackAction(player, monster));
            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(selector.CallCount, Is.EqualTo(2));
        }
    }
}