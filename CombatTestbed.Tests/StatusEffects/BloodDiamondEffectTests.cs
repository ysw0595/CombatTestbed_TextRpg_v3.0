using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Domain.StatusEffects
{
    [TestFixture]
    public class BloodDiamondEffectTests
    {
        [Test]
        public void 턴_종료마다_상대에게_스택만큼_피해를_주고_스택이_1씩_늘어나야_한다()
        {
            var owner = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var opponent = TestActorFactory.Create(name: "고블린", maxHp: 100);
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1));

            owner.EndTurn(opponent); // 1 피해, 스택 2
            owner.EndTurn(opponent); // 2 피해, 스택 3
            owner.EndTurn(opponent); // 3 피해, 스택 4

            Assert.That(opponent.Health.CurrentHp, Is.EqualTo(100 - (1 + 2 + 3)));
            Assert.That(owner.StatusEffects.GetStacks(StatusEffectType.BloodDiamond), Is.EqualTo(4));
            Assert.That(owner.Health.CurrentHp, Is.EqualTo(100)); // 소유자는 영향 없음
        }

        [Test]
        public void 이미_걸린_상태에서_다시_부여하면_스택이_쌓여_더_강한_피해를_주어야_한다()
        {
            var owner = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var opponent = TestActorFactory.Create(name: "고블린", maxHp: 100);
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1));
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1)); // 스킬 재사용 = 강화

            owner.EndTurn(opponent);

            Assert.That(opponent.Health.CurrentHp, Is.EqualTo(98)); // 스택 2 = 2 피해
            Assert.That(owner.StatusEffects.GetStacks(StatusEffectType.BloodDiamond), Is.EqualTo(3));
        }

        [Test]
        public void 상대가_없거나_이미_사망했으면_피해도_스택_증가도_없어야_한다()
        {
            var owner = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var opponent = TestActorFactory.Create(name: "고블린", maxHp: 100);
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1));

            owner.EndTurn(); // 상대 미지정
            opponent.ApplyTrueDamage(100); // 사망
            owner.EndTurn(opponent);

            Assert.That(owner.StatusEffects.GetStacks(StatusEffectType.BloodDiamond), Is.EqualTo(1));
            Assert.That(opponent.Health.CurrentHp, Is.EqualTo(0));
        }

        [Test]
        public void 상대의_방어력을_무시하고_피해를_주어야_한다()
        {
            var owner = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var opponent = TestActorFactory.Create(name: "고블린", maxHp: 100, defense: 50);
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1));

            owner.EndTurn(opponent);

            Assert.That(opponent.Health.CurrentHp, Is.EqualTo(99)); // 방어력 50이어도 1 피해
        }

        [Test]
        public void 턴_종료로_스택이_늘어날_때마다_EffectStacksChanged가_발행되어야_한다()
        {
            var owner = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var opponent = TestActorFactory.Create(name: "고블린", maxHp: 100);
            var log = new List<string>();
            owner.StatusEffects.EffectStacksChanged += effect => log.Add($"{effect.Type}:{effect.Stacks}");
            owner.StatusEffects.ApplyEffect(owner, new BloodDiamondEffect(1));

            owner.EndTurn(opponent);
            owner.EndTurn(opponent);

            Assert.That(log, Is.EqualTo(new[] { "BloodDiamond:2", "BloodDiamond:3" }));
        }
    }
}