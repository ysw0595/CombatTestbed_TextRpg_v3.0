using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Battle
{
    [TestFixture]
    public class ActorViewModelTests
    {
        [Test]
        public void 액터의_현재_상태를_이름_체력_공격력_상태이상_문구로_옮겨야_한다()
        {
            var actor = TestActorFactory.Create(name: "용사", maxHp: 100, attack: 30);
            actor.ApplyTrueDamage(40);
            actor.StatusEffects.ApplyEffect(actor, new PoisonEffect(3));
            actor.StatusEffects.ApplyEffect(actor, new BleedEffect(2));

            ActorViewModel model = ActorViewModel.From(actor);

            Assert.That(model.Name, Is.EqualTo("용사"));
            Assert.That(model.CurrentHp, Is.EqualTo(60));
            Assert.That(model.MaxHp, Is.EqualTo(100));
            Assert.That(model.Attack, Is.EqualTo(30));
            Assert.That(model.Statuses, Is.EqualTo(new[] { "중독 3", "출혈 2" }));
            Assert.That(model.HpRatio, Is.EqualTo(0.6f).Within(0.0001f));
        }

        [Test]
        public void 체력_비율은_0에서_1_사이로_제한되고_잘못된_입력은_거부되어야_한다()
        {
            var none = new List<string>();

            Assert.That(new ActorViewModel("a", 50, 0, 1, none).HpRatio, Is.EqualTo(0f));   // 최대 체력 0
            Assert.That(new ActorViewModel("a", 150, 100, 1, none).HpRatio, Is.EqualTo(1f)); // 초과
            Assert.That(new ActorViewModel("a", -5, 100, 1, none).HpRatio, Is.EqualTo(0f));  // 음수
            Assert.Throws<ArgumentNullException>(() => new ActorViewModel("a", 1, 1, 1, null));
            Assert.Throws<ArgumentNullException>(() => ActorViewModel.From(null));
        }
    }
}