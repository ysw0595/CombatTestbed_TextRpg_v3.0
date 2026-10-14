using NUnit.Framework;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Domain.StatusEffects
{
    [TestFixture]
    public class StatusEffectContainerEventTests
    {
        // 컨테이너의 세 이벤트를 한 로그에 순서대로 기록하는 도우미
        private EventLog Record(BattleActor actor)
        {
            var log = new EventLog();
            actor.StatusEffects.EffectApplied += e => log.Add($"Applied:{e.Type}:{e.Stacks}");
            actor.StatusEffects.EffectStacksChanged += e => log.Add($"StacksChanged:{e.Type}:{e.Stacks}");
            actor.StatusEffects.EffectRemoved += e => log.Add($"Removed:{e.Type}");
            return log;
        }

        [Test]
        public void 처음_부여하면_EffectApplied만_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);

            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(3));

            Assert.That(log.Entries, Is.EqualTo(new[] { "Applied:Poison:3" }));
        }

        [Test]
        public void 같은_종류를_추가_부여하면_합산된_스택과_함께_EffectStacksChanged가_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);

            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(3));
            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(2));

            Assert.That(log.Entries, Is.EqualTo(new[] { "Applied:Poison:3", "StacksChanged:Poison:5" }));
        }

        [Test]
        public void 만료된_효과나_null을_부여하면_이벤트_없이_무시되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);

            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(0));
            actor.StatusEffects.ApplyEffect(actor, null);

            Assert.That(log.Entries, Is.Empty);
        }

        [Test]
        public void 독은_턴_종료마다_스택_변화가_통보되고_소멸할_때는_Removed만_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);
            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(2));

            actor.EndTurn(); // 2 -> 1
            actor.EndTurn(); // 1 -> 0 (소멸)

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Applied:Poison:2",
                "StacksChanged:Poison:1",
                "Removed:Poison"   // 0스택 StacksChanged는 발행되지 않음
            }));
        }

        [Test]
        public void 출혈은_턴이_지나도_스택이_변하지_않으므로_StacksChanged가_발행되지_않아야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);
            actor.StatusEffects.ApplyEffect(actor, new BleedEffect(4));

            actor.StartTurn();
            actor.EndTurn();
            actor.EndTurn();

            Assert.That(log.Entries, Is.EqualTo(new[] { "Applied:Bleed:4" }));
        }

        [Test]
        public void 마비는_턴_종료마다_스택이_줄고_소진되면_Removed가_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);
            actor.StatusEffects.ApplyEffect(actor, new ParalysisEffect(2));

            actor.EndTurn(); // 2 -> 1
            actor.EndTurn(); // 1 -> 0 (소멸)

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Applied:Paralysis:2",
                "StacksChanged:Paralysis:1",
                "Removed:Paralysis"
            }));
        }

        [Test]
        public void 정화하면_EffectRemoved가_한번_발행되고_없는_효과의_정화는_이벤트가_없어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);
            actor.StatusEffects.ApplyEffect(actor, new BleedEffect(5));

            bool first = actor.StatusEffects.RemoveEffect(actor, StatusEffectType.Bleed);
            bool second = actor.StatusEffects.RemoveEffect(actor, StatusEffectType.Bleed);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(log.Entries, Is.EqualTo(new[] { "Applied:Bleed:5", "Removed:Bleed" }));
        }

        [Test]
        public void Clear하면_걸려_있던_모든_효과에_대해_EffectRemoved가_발행되어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            var log = Record(actor);
            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(3));
            actor.StatusEffects.ApplyEffect(actor, new BleedEffect(2));

            actor.StatusEffects.Clear(actor);

            // 순서는 규약이 아니므로 포함 여부와 개수만 확인
            Assert.That(log.Entries.Count, Is.EqualTo(4)); // Applied 2 + Removed 2
            Assert.That(log.Entries, Does.Contain("Removed:Poison"));
            Assert.That(log.Entries, Does.Contain("Removed:Bleed"));
            Assert.That(actor.StatusEffects.HasEffect(StatusEffectType.Poison), Is.False);
            Assert.That(actor.StatusEffects.HasEffect(StatusEffectType.Bleed), Is.False);
        }

        [Test]
        public void 이벤트_핸들러에서는_이미_갱신된_컨테이너_상태를_읽을_수_있어야_한다()
        {
            var actor = TestActorFactory.Create(maxHp: 100);
            bool hasEffectWhenApplied = false;
            int stacksSeenWhenChanged = -1;
            bool hasEffectWhenRemoved = true;
            actor.StatusEffects.EffectApplied +=
                e => hasEffectWhenApplied = actor.StatusEffects.HasEffect(e.Type);
            actor.StatusEffects.EffectStacksChanged +=
                e => stacksSeenWhenChanged = actor.StatusEffects.GetStacks(e.Type);
            actor.StatusEffects.EffectRemoved +=
                e => hasEffectWhenRemoved = actor.StatusEffects.HasEffect(e.Type);

            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(2));
            actor.EndTurn(); // 2 -> 1
            actor.EndTurn(); // 1 -> 0 (소멸)

            Assert.That(hasEffectWhenApplied, Is.True);
            Assert.That(stacksSeenWhenChanged, Is.EqualTo(1));
            Assert.That(hasEffectWhenRemoved, Is.False);
        }
    }
}