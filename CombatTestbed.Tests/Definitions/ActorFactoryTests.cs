using System;
using NUnit.Framework;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Definitions
{
    [TestFixture]
    public class ActorFactoryTests
    {
        private static SkillDefinition Skill(string id)
        {
            return new SkillDefinition(id, id, new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
            });
        }

        [Test]
        public void 정의의_스탯과_이름_역할로_체력이_가득_찬_액터를_만들어야_한다()
        {
            var definition = new ActorDefinition("goblin", "고블린", ActorRole.Enemy, maxHp: 50, attack: 7, defense: 3);

            BattleActor actor = ActorFactory.Create(definition);

            Assert.That(actor.Id, Is.EqualTo("goblin"));
            Assert.That(actor.Name, Is.EqualTo("고블린"));
            Assert.That(actor.Role, Is.EqualTo(ActorRole.Enemy));
            Assert.That(actor.Health.MaxHp, Is.EqualTo(50));
            Assert.That(actor.Health.CurrentHp, Is.EqualTo(50));
            Assert.That(actor.Stats.GetValue(StatType.Attack), Is.EqualTo(7));
            Assert.That(actor.Stats.GetValue(StatType.Defense), Is.EqualTo(3));
        }

        [Test]
        public void 같은_정의로_여러_번_만들면_서로_독립된_액터여야_하고_인스턴스_Id를_지정할_수_있어야_한다()
        {
            var definition = new ActorDefinition("goblin", "고블린", ActorRole.Enemy, maxHp: 50, attack: 7);

            BattleActor first = ActorFactory.Create(definition, "goblin_1");
            BattleActor second = ActorFactory.Create(definition, "goblin_2");
            first.ApplyTrueDamage(20);

            Assert.That(first.Id, Is.EqualTo("goblin_1"));
            Assert.That(second.Id, Is.EqualTo("goblin_2"));
            Assert.That(first.Health.CurrentHp, Is.EqualTo(30));
            Assert.That(second.Health.CurrentHp, Is.EqualTo(50)); // 서로 영향 없음
        }

        [Test]
        public void 행동_스킬이_없으면_일반공격_선택기를_스킬이_있으면_순환_선택기를_만들어야_한다()
        {
            var plain = new ActorDefinition("plain", "일반", ActorRole.Enemy, 10, 1);
            var boss = new ActorDefinition("boss", "보스", ActorRole.Enemy, 10, 1,
                behaviorSkills: new[] { Skill("a"), Skill("b") });
            var self = TestActorFactory.Create(name: "자신");
            var opponent = TestActorFactory.Create(name: "상대");

            IActionSelector plainSelector = ActorFactory.CreateSelector(plain);
            IActionSelector bossSelector = ActorFactory.CreateSelector(boss, startIndex: 1);

            Assert.That(plainSelector, Is.TypeOf<BasicAttackSelector>());
            Assert.That(bossSelector, Is.TypeOf<CyclingSkillSelector>());
            var first = (SkillAction)bossSelector.SelectAction(self, opponent);
            Assert.That(first.Definition.Id, Is.EqualTo("b")); // 시작 순번 1
        }

        [Test]
        public void 정의가_null이면_예외가_발생해야_한다()
        {
            Assert.Throws<ArgumentNullException>(() => ActorFactory.Create(null));
            Assert.Throws<ArgumentNullException>(() => ActorFactory.CreateSelector(null));
        }
    }
}