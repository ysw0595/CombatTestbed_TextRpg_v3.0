using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Combat
{
    [TestFixture]
    public class TurnManagerEventTests
    {
        private static string WinnerName(BattleActor winner)
        {
            return winner == null ? "None" : winner.Name;
        }

        private static string Describe(ActionResult result)
        {
            return result.IsSuccess ? "Success" : "Failure";
        }

        // 전투 관련 이벤트를 모두 한 로그에 순서대로 기록하는 도우미
        private TurnManager CreateBattle(BattleActor player, BattleActor monster, EventLog log)
        {
            var turnManager = new TurnManager(player, monster, new ActionPipeline());
            turnManager.RoundStarted += round => log.Add($"RoundStarted:{round}");
            turnManager.TurnStarted += actor => log.Add($"TurnStarted:{actor.Name}");
            turnManager.TurnEnded += actor => log.Add($"TurnEnded:{actor.Name}");
            turnManager.BattleEnded += winner => log.Add($"BattleEnded:{WinnerName(winner)}");
            turnManager.Pipeline.ActionResolved +=
                r => log.Add($"ActionResolved:{r.Source.Name}:{Describe(r)}");
            turnManager.TurnSkipped += actor => log.Add($"TurnSkipped:{actor.Name}");
            return turnManager;
        }

        [Test]
        public void 정상_1라운드는_시작_플레이어턴_몬스터턴_순서로_이벤트가_발행되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "RoundStarted:1",
                "TurnStarted:용사",
                "ActionResolved:용사:Success",
                "TurnEnded:용사",
                "TurnStarted:고블린",
                "ActionResolved:고블린:Success",
                "TurnEnded:고블린"
            }));
        }

        [Test]
        public void 라운드가_이어지면_RoundStarted의_라운드_번호가_증가해야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));
            turnManager.ExecuteRound(new AttackAction(player, monster));

            var roundEvents = log.Entries.Where(e => e.StartsWith("RoundStarted")).ToArray();
            Assert.That(roundEvents, Is.EqualTo(new[] { "RoundStarted:1", "RoundStarted:2" }));
        }

        [Test]
        public void 플레이어가_몬스터를_처치하면_몬스터_턴_없이_BattleEnded가_발행되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 10, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "RoundStarted:1",
                "TurnStarted:용사",
                "ActionResolved:용사:Success",
                "TurnEnded:용사",
                "BattleEnded:용사"
            }));
        }

        [Test]
        public void 몬스터의_반격으로_플레이어가_사망하면_몬스터_턴_종료_뒤_BattleEnded가_발행되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 10, attack: 5);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "RoundStarted:1",
                "TurnStarted:용사",
                "ActionResolved:용사:Success",
                "TurnEnded:용사",
                "TurnStarted:고블린",
                "ActionResolved:고블린:Success",
                "TurnEnded:고블린",   // 전투가 끝나 턴 종료 처리가 생략되어도 TurnEnded는 발행
                "BattleEnded:고블린"
            }));
        }

        [Test]
        public void 턴_종료_독_피해로_플레이어가_사망해도_TurnEnded_다음에_BattleEnded가_발행되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 3, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            player.StatusEffects.ApplyEffect(player, new PoisonEffect(3));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "RoundStarted:1",
                "TurnStarted:용사",
                "ActionResolved:용사:Success",
                "TurnEnded:용사",       // 독 틱으로 사망한 뒤 턴 종료
                "BattleEnded:고블린"     // 몬스터 턴은 시작되지 않음
            }));
        }

        [Test]
        public void 마비로_행동할_수_없으면_ActionResolved_없이_TurnSkipped가_발행되고_턴의_시작과_끝은_쌍으로_발행되어야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 100, attack: 10);
            player.StatusEffects.ApplyEffect(player, new ParalysisEffect(1));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.ExecuteRound(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
        "RoundStarted:1",
        "TurnStarted:용사",
        "TurnSkipped:용사",          // 행동 시도(ActionResolved) 대신 스킵 통보
        "TurnEnded:용사",
        "TurnStarted:고블린",
        "ActionResolved:고블린:Success",
        "TurnEnded:고블린"
    }));
        }

        [Test]
        public void 전투_종료_후_라운드를_진행하면_예외가_발생하고_이벤트는_더_발행되지_않아야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 100, attack: 20);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 10, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);
            turnManager.ExecuteRound(new AttackAction(player, monster)); // 전투 종료
            int countBefore = log.Entries.Count;

            Assert.Throws<InvalidOperationException>(
                () => turnManager.ExecuteRound(new AttackAction(player, monster)));

            Assert.That(log.Entries.Count, Is.EqualTo(countBefore));
            Assert.That(log.Entries.Count(e => e.StartsWith("BattleEnded")), Is.EqualTo(1));
        }

        [Test]
        public void null_액션으로_라운드를_진행하면_예외가_발생하고_이벤트는_발행되지_않아야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            Assert.Throws<ArgumentNullException>(() => turnManager.ExecuteRound(null));

            Assert.That(log.Entries, Is.Empty);
        }
    }
}