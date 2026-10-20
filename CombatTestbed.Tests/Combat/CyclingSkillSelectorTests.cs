using System;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Combat
{
    [TestFixture]
    public class CyclingSkillSelectorTests
    {
        // 피해량이 서로 다른 스킬 (공격력 10 기준 10 / 20 / 30)
        private static SkillDefinition DamageSkill(string id, float coefficient)
        {
            return new SkillDefinition(id, id, new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, coefficient)
            });
        }

        private static string SelectedId(IActionSelector selector, BattleActor self, BattleActor opponent)
        {
            return ((SkillAction)selector.SelectAction(self, opponent)).Definition.Id;
        }

        [Test]
        public void 스킬을_목록_순서대로_고르고_마지막_다음에는_처음으로_돌아가야_한다()
        {
            var selector = new CyclingSkillSelector(new[]
            {
                DamageSkill("a", 1f), DamageSkill("b", 2f), DamageSkill("c", 3f)
            });
            var self = TestActorFactory.Create(name: "보스");
            var opponent = TestActorFactory.Create(name: "용사");

            var selected = new[]
            {
                SelectedId(selector, self, opponent), SelectedId(selector, self, opponent),
                SelectedId(selector, self, opponent), SelectedId(selector, self, opponent),
                SelectedId(selector, self, opponent)
            };

            Assert.That(selected, Is.EqualTo(new[] { "a", "b", "c", "a", "b" }));
        }

        [Test]
        public void 시작_순번을_지정하면_그_스킬부터_순환해야_한다()
        {
            var selector = new CyclingSkillSelector(new[]
            {
                DamageSkill("a", 1f), DamageSkill("b", 2f), DamageSkill("c", 3f)
            }, startIndex: 1);
            var self = TestActorFactory.Create(name: "보스");
            var opponent = TestActorFactory.Create(name: "용사");

            var selected = new[]
            {
                SelectedId(selector, self, opponent), SelectedId(selector, self, opponent),
                SelectedId(selector, self, opponent)
            };

            Assert.That(selected, Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void 잘못된_입력으로는_선택기를_만들_수_없어야_한다()
        {
            var skills = new[] { DamageSkill("a", 1f), DamageSkill("b", 2f), DamageSkill("c", 3f) };

            Assert.Throws<ArgumentNullException>(() => new CyclingSkillSelector(null));
            Assert.Throws<ArgumentException>(() => new CyclingSkillSelector(new SkillDefinition[0]));
            Assert.Throws<ArgumentException>(() => new CyclingSkillSelector(new SkillDefinition[] { null }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CyclingSkillSelector(skills, startIndex: 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CyclingSkillSelector(skills, startIndex: -1));
        }

        [Test]
        public void 선택한_행동에는_자신이_시전자로_상대가_대상으로_전달되어야_한다()
        {
            var selector = new CyclingSkillSelector(new[] { DamageSkill("a", 1f) });
            var self = TestActorFactory.Create(name: "보스");
            var opponent = TestActorFactory.Create(name: "용사");

            ICombatAction action = selector.SelectAction(self, opponent);

            Assert.That(action.Source, Is.SameAs(self));
            Assert.That(action.Target, Is.SameAs(opponent));
        }

        [Test]
        public void 전투에_연결하면_보스가_매_턴_스킬을_순환해서_사용해야_한다()
        {
            // 공격력 10인 보스가 배율 1 / 2 / 3 스킬을 순환 -> 턴마다 10, 20, 30, 10 피해
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 1000, attack: 1);
            var boss = TestActorFactory.Create(name: "보스", role: ActorRole.Enemy, maxHp: 100000, attack: 10);
            var selector = new CyclingSkillSelector(new[]
            {
                DamageSkill("a", 1f), DamageSkill("b", 2f), DamageSkill("c", 3f)
            });
            var turnManager = new TurnManager(player, boss, new ActionPipeline(), selector);

            for (int i = 0; i < 4; i++)
            {
                turnManager.ExecuteRound(new AttackAction(player, boss));
            }

            Assert.That(player.Health.CurrentHp, Is.EqualTo(1000 - (10 + 20 + 30 + 10)));
        }

        [Test]
        public void 보스가_마비로_턴을_건너뛰면_스킬_순번은_넘어가지_않아야_한다()
        {
            var player = TestActorFactory.Create(name: "용사", role: ActorRole.Player, maxHp: 1000, attack: 1);
            var boss = TestActorFactory.Create(name: "보스", role: ActorRole.Enemy, maxHp: 100000, attack: 10);
            boss.StatusEffects.ApplyEffect(boss, new ParalysisEffect(1));
            var selector = new CyclingSkillSelector(new[] { DamageSkill("a", 1f), DamageSkill("b", 2f) });
            var turnManager = new TurnManager(player, boss, new ActionPipeline(), selector);

            turnManager.ExecuteRound(new AttackAction(player, boss)); // 1라운드: 보스 마비로 스킵
            turnManager.ExecuteRound(new AttackAction(player, boss)); // 2라운드: 첫 번째 스킬(10 피해)

            Assert.That(player.Health.CurrentHp, Is.EqualTo(990));
        }
    }
}