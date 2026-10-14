using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Pipeline
{
    [TestFixture]
    public class ActionPipelineEventTests
    {
        private ActionPipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _pipeline = new ActionPipeline();
        }

        private string Describe(ActionResult result)
        {
            return result.IsSuccess ? "Success" : "Failure";
        }

        [Test]
        public void 성공한_행동은_ActionResolved가_반환값과_같은_성공_결과로_한번_발행되어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "용사", attack: 25);
            var defender = TestActorFactory.Create(name: "슬라임", maxHp: 100, defense: 10);
            var raised = new List<ActionResult>();
            _pipeline.ActionResolved += raised.Add;

            ActionResult result = _pipeline.Execute(new AttackAction(attacker, defender));

            Assert.That(raised.Count, Is.EqualTo(1));
            Assert.That(raised[0], Is.SameAs(result));
            Assert.That(raised[0].IsSuccess, Is.True);
            Assert.That(raised[0].DamageDealt, Is.EqualTo(15)); // 25 - 방어 10
        }

        [Test]
        public void 사전검증에서_실패한_행동도_실패_결과로_한번_발행되고_피해_이벤트는_없어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "용사", attack: 25);
            var defender = TestActorFactory.Create(name: "슬라임", maxHp: 100);
            attacker.StatusEffects.ApplyEffect(attacker, new ParalysisEffect(1));
            var log = new EventLog();
            defender.DamageTaken += (a, amount) => log.Add($"DamageTaken:{amount}");
            _pipeline.ActionResolved += r => log.Add($"ActionResolved:{Describe(r)}");

            _pipeline.Execute(new AttackAction(attacker, defender));

            Assert.That(log.Entries, Is.EqualTo(new[] { "ActionResolved:Failure" }));
        }

        [Test]
        public void 치명적인_공격은_DamageTaken_Died_ActionResolved_순서로_발행되어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "용사", attack: 25);
            var defender = TestActorFactory.Create(name: "슬라임", maxHp: 10, defense: 0);
            var log = new EventLog();
            defender.DamageTaken += (a, amount) => log.Add($"DamageTaken:{amount}");
            defender.Died += a => log.Add("Died");
            _pipeline.ActionResolved += r => log.Add($"ActionResolved:{Describe(r)}");

            _pipeline.Execute(new AttackAction(attacker, defender));

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "DamageTaken:10",
                "Died",
                "ActionResolved:Success"
            }));
        }

        [Test]
        public void null_액션은_예외를_던지고_ActionResolved는_발행되지_않아야_한다()
        {
            int raisedCount = 0;
            _pipeline.ActionResolved += r => raisedCount++;

            Assert.Throws<ArgumentNullException>(() => _pipeline.Execute(null));

            Assert.That(raisedCount, Is.EqualTo(0));
        }

        [Test]
        public void ActionResolved_핸들러에서는_이미_반영된_체력을_읽을_수_있어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "용사", attack: 15);
            var defender = TestActorFactory.Create(name: "슬라임", maxHp: 100, defense: 0);
            int hpSeenInHandler = -1;
            _pipeline.ActionResolved += r => hpSeenInHandler = r.Target.Health.CurrentHp;

            _pipeline.Execute(new AttackAction(attacker, defender));

            Assert.That(hpSeenInHandler, Is.EqualTo(85));
        }
    }
}