using System;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Domain.StatusEffects
{
    [TestFixture]
    public class TurnContextTests
    {
        [Test]
        public void 컨텍스트는_소유자와_상대를_보관하고_소유자가_null이면_생성할_수_없어야_한다()
        {
            var owner = TestActorFactory.Create(name: "소유자");
            var opponent = TestActorFactory.Create(name: "상대");

            var context = new TurnContext(owner, opponent);
            var withoutOpponent = new TurnContext(owner, null);

            Assert.That(context.Owner, Is.SameAs(owner));
            Assert.That(context.Opponent, Is.SameAs(opponent));
            Assert.That(withoutOpponent.Opponent, Is.Null);
            Assert.Throws<ArgumentNullException>(() => new TurnContext(null, opponent));
        }

        [Test]
        public void StartTurn은_소유자와_상대를_턴_시작_훅에_전달해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy);
            var stub = new OpponentDamageEffectStub(damage: 0);
            player.StatusEffects.ApplyEffect(player, stub);

            player.StartTurn(monster);

            Assert.That(stub.LastTurnStartContext.HasValue, Is.True);
            Assert.That(stub.LastTurnStartContext.Value.Owner, Is.SameAs(player));
            Assert.That(stub.LastTurnStartContext.Value.Opponent, Is.SameAs(monster));
        }

        [Test]
        public void EndTurn은_소유자와_상대를_턴_종료_훅에_전달해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy);
            var stub = new OpponentDamageEffectStub(damage: 0);
            player.StatusEffects.ApplyEffect(player, stub);

            player.EndTurn(monster);

            Assert.That(stub.LastTurnEndContext.HasValue, Is.True);
            Assert.That(stub.LastTurnEndContext.Value.Owner, Is.SameAs(player));
            Assert.That(stub.LastTurnEndContext.Value.Opponent, Is.SameAs(monster));
        }

        [Test]
        public void 상대를_아는_효과는_턴_종료_때_상대에게_방어무시_피해를_줄_수_있어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, defense: 50);
            player.StatusEffects.ApplyEffect(player, new OpponentDamageEffectStub(damage: 5));

            player.EndTurn(monster);

            Assert.That(monster.Health.CurrentHp, Is.EqualTo(95)); // 방어력 50이어도 고정 피해 5
            Assert.That(player.Health.CurrentHp, Is.EqualTo(100)); // 소유자는 영향 없음
        }

        [Test]
        public void 상대를_지정하지_않으면_Opponent는_null이고_예외_없이_동작해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player);
            var stub = new OpponentDamageEffectStub(damage: 5);
            player.StatusEffects.ApplyEffect(player, stub);

            Assert.DoesNotThrow(() => player.EndTurn());

            Assert.That(stub.LastTurnEndContext.Value.Owner, Is.SameAs(player));
            Assert.That(stub.LastTurnEndContext.Value.Opponent, Is.Null);
        }

        [Test]
        public void TurnManager는_양쪽_턴에서_서로를_상대로_전달해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var playerStub = new OpponentDamageEffectStub(damage: 5);
            var monsterStub = new OpponentDamageEffectStub(damage: 3);
            player.StatusEffects.ApplyEffect(player, playerStub);
            monster.StatusEffects.ApplyEffect(monster, monsterStub);
            var turnManager = new TurnManager(player, monster, new ActionPipeline());

            turnManager.ExecuteRound(new AttackAction(player, monster));

            // 몬스터: 20(공격) + 5(플레이어 효과) = 25 피해 -> 75
            // 플레이어: 10(반격) + 3(몬스터 효과) = 13 피해 -> 87
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(75));
            Assert.That(player.Health.CurrentHp, Is.EqualTo(87));
            Assert.That(playerStub.LastTurnEndContext.Value.Opponent, Is.SameAs(monster));
            Assert.That(monsterStub.LastTurnEndContext.Value.Opponent, Is.SameAs(player));
        }
    }
}