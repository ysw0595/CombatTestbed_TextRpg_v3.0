using System;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Combat
{
    [TestFixture]
    public class TurnManagerFsmTests
    {
        private static string WinnerName(BattleActor winner)
        {
            return winner == null ? "None" : winner.Name;
        }

        // 전투 단계 변화와 진행 이벤트를 한 로그에 모아서 서로의 순서를 검증
        private TurnManager CreateBattle(BattleActor player, BattleActor monster, EventLog log)
        {
            var turnManager = new TurnManager(player, monster, new ActionPipeline());
            turnManager.PhaseChanged += (previous, next) => log.Add($"PhaseChanged:{previous}->{next}");
            turnManager.RoundStarted += round => log.Add($"RoundStarted:{round}");
            turnManager.TurnStarted += actor => log.Add($"TurnStarted:{actor.Name}");
            turnManager.TurnEnded += actor => log.Add($"TurnEnded:{actor.Name}");
            turnManager.BattleEnded += winner => log.Add($"BattleEnded:{WinnerName(winner)}");
            return turnManager;
        }

        private BattleActor CreatePlayer(int maxHp = 100, int attack = 20)
        {
            return TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: maxHp, attack: attack);
        }

        private BattleActor CreateMonster(int maxHp = 100, int attack = 10)
        {
            return TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: maxHp, attack: attack);
        }

        [Test]
        public void 시작_전에는_NotStarted이고_Start하면_플레이어_입력_대기_단계가_되어야_한다()
        {
            var log = new EventLog();
            var turnManager = CreateBattle(CreatePlayer(), CreateMonster(), log);
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.NotStarted));

            turnManager.Start();

            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(log.Entries, Is.EqualTo(new[] { "PhaseChanged:NotStarted->PlayerTurn" }));
        }

        [Test]
        public void Start를_두번_호출하면_예외가_발생해야_한다()
        {
            var turnManager = CreateBattle(CreatePlayer(), CreateMonster(), new EventLog());
            turnManager.Start();

            Assert.Throws<InvalidOperationException>(() => turnManager.Start());
        }

        [Test]
        public void 시작하기_전에_행동을_제출하면_예외가_발생해야_한다()
        {
            var player = CreatePlayer();
            var monster = CreateMonster();
            var turnManager = CreateBattle(player, monster, new EventLog());

            Assert.Throws<InvalidOperationException>(
                () => turnManager.SubmitPlayerAction(new AttackAction(player, monster)));
        }

        [Test]
        public void 한_라운드를_진행하면_단계_통보가_각_단계의_이벤트보다_먼저_발행되고_다시_입력_대기가_되어야_한다()
        {
            var player = CreatePlayer();
            var monster = CreateMonster();
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);
            turnManager.Start();

            turnManager.SubmitPlayerAction(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "PhaseChanged:NotStarted->PlayerTurn",
                "RoundStarted:1",
                "TurnStarted:용사",
                "TurnEnded:용사",
                "PhaseChanged:PlayerTurn->EnemyTurn",   // 적 턴 이벤트보다 먼저
                "TurnStarted:고블린",
                "TurnEnded:고블린",
                "PhaseChanged:EnemyTurn->PlayerTurn"
            }));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
        }

        [Test]
        public void 플레이어가_몬스터를_처치하면_적_턴_없이_전투_종료_단계가_되어야_한다()
        {
            var player = CreatePlayer(attack: 20);
            var monster = CreateMonster(maxHp: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);
            turnManager.Start();

            turnManager.SubmitPlayerAction(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "PhaseChanged:NotStarted->PlayerTurn",
                "RoundStarted:1",
                "TurnStarted:용사",
                "TurnEnded:용사",
                "PhaseChanged:PlayerTurn->BattleOver",
                "BattleEnded:용사"
            }));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.BattleOver));
        }

        [Test]
        public void 몬스터의_반격으로_플레이어가_사망하면_적_턴_뒤_전투_종료_단계가_되어야_한다()
        {
            var player = CreatePlayer(maxHp: 10, attack: 5);
            var monster = CreateMonster(maxHp: 100, attack: 10);
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);
            turnManager.Start();

            turnManager.SubmitPlayerAction(new AttackAction(player, monster));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "PhaseChanged:NotStarted->PlayerTurn",
                "RoundStarted:1",
                "TurnStarted:용사",
                "TurnEnded:용사",
                "PhaseChanged:PlayerTurn->EnemyTurn",
                "TurnStarted:고블린",
                "TurnEnded:고블린",
                "PhaseChanged:EnemyTurn->BattleOver",
                "BattleEnded:고블린"
            }));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.BattleOver));
        }

        [Test]
        public void 전투가_종료된_뒤_행동을_제출하면_예외가_발생하고_단계는_유지되어야_한다()
        {
            var player = CreatePlayer(attack: 20);
            var monster = CreateMonster(maxHp: 10);
            var turnManager = CreateBattle(player, monster, new EventLog());
            turnManager.Start();
            turnManager.SubmitPlayerAction(new AttackAction(player, monster)); // 전투 종료

            Assert.Throws<InvalidOperationException>(
                () => turnManager.SubmitPlayerAction(new AttackAction(player, monster)));

            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.BattleOver));
        }

        [Test]
        public void Start를_먼저_호출한_뒤에도_ExecuteRound를_정상적으로_사용할_수_있어야_한다()
        {
            var player = CreatePlayer();
            var monster = CreateMonster();
            var turnManager = CreateBattle(player, monster, new EventLog());
            turnManager.Start();

            Assert.DoesNotThrow(() => turnManager.ExecuteRound(new AttackAction(player, monster)));

            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
        }

        [Test]
        public void null_행동을_제출하면_예외가_발생하고_입력_대기_단계가_유지되어야_한다()
        {
            var log = new EventLog();
            var turnManager = CreateBattle(CreatePlayer(), CreateMonster(), log);
            turnManager.Start();

            Assert.Throws<ArgumentNullException>(() => turnManager.SubmitPlayerAction(null));

            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(log.Entries, Is.EqualTo(new[] { "PhaseChanged:NotStarted->PlayerTurn" })); // 추가 이벤트 없음
        }
    }
}