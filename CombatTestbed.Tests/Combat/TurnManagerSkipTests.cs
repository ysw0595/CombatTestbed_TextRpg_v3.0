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
    public class TurnManagerSkipTests
    {
        private static string WinnerName(BattleActor winner)
        {
            return winner == null ? "None" : winner.Name;
        }

        private TurnManager CreateBattle(
            BattleActor player, BattleActor monster, EventLog log, IActionSelector selector = null)
        {
            var turnManager = new TurnManager(player, monster, new ActionPipeline(), selector);
            turnManager.PhaseChanged += (previous, next) => log.Add($"PhaseChanged:{previous}->{next}");
            turnManager.RoundStarted += round => log.Add($"RoundStarted:{round}");
            turnManager.TurnStarted += actor => log.Add($"TurnStarted:{actor.Name}");
            turnManager.TurnSkipped += actor => log.Add($"TurnSkipped:{actor.Name}");
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
        public void 플레이어가_마비면_입력을_기다리지_않고_턴을_넘긴_뒤_다음_라운드에서_입력을_기다려야_한다()
        {
            var player = CreatePlayer();
            var monster = CreateMonster();
            player.StatusEffects.ApplyEffect(player, new ParalysisEffect(1));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.Start();

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "PhaseChanged:NotStarted->PlayerTurn",
                "RoundStarted:1",
                "TurnStarted:용사",
                "TurnSkipped:용사",
                "TurnEnded:용사",
                "PhaseChanged:PlayerTurn->EnemyTurn",
                "TurnStarted:고블린",
                "TurnEnded:고블린",
                "PhaseChanged:EnemyTurn->PlayerTurn"
            }));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(2));
            Assert.That(player.CanAct, Is.True);                  // 마비 1스택 소모 후 해제
            Assert.That(player.Health.CurrentHp, Is.EqualTo(90)); // 스킵한 라운드에도 몬스터는 공격
        }

        [Test]
        public void 마비_2스택이면_두_라운드_연속_자동으로_넘기고_그_뒤에_입력을_기다려야_한다()
        {
            var player = CreatePlayer();
            var monster = CreateMonster();
            player.StatusEffects.ApplyEffect(player, new ParalysisEffect(2));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.Start();

            Assert.That(log.Entries.Count(e => e == "TurnSkipped:용사"), Is.EqualTo(2));
            Assert.That(log.Entries, Does.Contain("RoundStarted:1"));
            Assert.That(log.Entries, Does.Contain("RoundStarted:2"));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(3));
            Assert.That(player.Health.CurrentHp, Is.EqualTo(80)); // 몬스터 공격 2회
        }

        [Test]
        public void 행동_제출_직후_플레이어가_마비되면_같은_호출_안에서_다음_라운드를_자동으로_넘겨야_한다()
        {
            var player = CreatePlayer(attack: 20);
            var monster = CreateMonster(maxHp: 100, attack: 10);
            int selectCount = 0;
            // 첫 몬스터 턴에만 마비를 걸고, 이후에는 일반 공격
            var selector = new StubActionSelector((self, opponent) =>
                selectCount++ == 0
                    ? (ICombatAction)new ParalyzeActionStub(self, opponent, 1)
                    : new AttackAction(self, opponent));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log, selector);
            turnManager.Start();

            turnManager.SubmitPlayerAction(new AttackAction(player, monster));

            // 1라운드: 플레이어 공격 -> 몬스터가 마비 부여 / 2라운드: 플레이어 스킵 -> 몬스터 공격
            Assert.That(log.Entries.Count(e => e == "TurnSkipped:용사"), Is.EqualTo(1));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
            Assert.That(turnManager.CurrentRound, Is.EqualTo(3));
            Assert.That(monster.Health.CurrentHp, Is.EqualTo(80)); // 플레이어 공격은 1회만
            Assert.That(player.Health.CurrentHp, Is.EqualTo(90));  // 마비 부여 턴은 피해 없음, 이후 일반 공격 1회
            Assert.That(selectCount, Is.EqualTo(2));
        }

        [Test]
        public void 몬스터가_마비면_행동_선택_없이_턴을_넘기고_마비가_소모되어야_한다()
        {
            var player = CreatePlayer(attack: 20);
            var monster = CreateMonster(maxHp: 100, attack: 10);
            monster.StatusEffects.ApplyEffect(monster, new ParalysisEffect(1));
            var selector = new StubActionSelector((self, opponent) => new AttackAction(self, opponent));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log, selector);
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
                "TurnSkipped:고블린",
                "TurnEnded:고블린",
                "PhaseChanged:EnemyTurn->PlayerTurn"
            }));
            Assert.That(selector.CallCount, Is.EqualTo(0));        // 행동 선택 생략
            Assert.That(player.Health.CurrentHp, Is.EqualTo(100)); // 몬스터는 공격하지 못함
            Assert.That(monster.CanAct, Is.True);                  // 마비 해제
        }

        [Test]
        public void 마비된_플레이어가_턴_종료_독_피해로_사망하면_스킵_턴_뒤_전투가_종료되어야_한다()
        {
            var player = CreatePlayer(maxHp: 3);
            var monster = CreateMonster();
            player.StatusEffects.ApplyEffect(player, new ParalysisEffect(1));
            player.StatusEffects.ApplyEffect(player, new PoisonEffect(3));
            var log = new EventLog();
            var turnManager = CreateBattle(player, monster, log);

            turnManager.Start();

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "PhaseChanged:NotStarted->PlayerTurn",
                "RoundStarted:1",
                "TurnStarted:용사",
                "TurnSkipped:용사",
                "TurnEnded:용사",
                "PhaseChanged:PlayerTurn->BattleOver",   // 적 턴 없이 종료
                "BattleEnded:고블린"
            }));
            Assert.That(turnManager.Phase, Is.EqualTo(BattlePhase.BattleOver));
        }
    }
}