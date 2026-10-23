using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Content
{
    [TestFixture]
    public class VampireSkillsTests
    {
        [Test]
        public void BatFire는_공격력만큼_피해를_준다()
        {
            var vampire = TestActorFactory.Create(name: "흡혈귀", attack: 300);
            var target = TestActorFactory.Create(name: "고블린", maxHp: 1000, defense: 0);

            var result = new SkillAction(DefaultSkills.BatFire, vampire, target).Execute();

            Assert.That(result.DamageDealt, Is.EqualTo(300));
            Assert.That(target.Health.CurrentHp, Is.EqualTo(700));
        }

        [Test]
        public void BloodExplosion은_최대체력의_30퍼센트를_자해하고_공격력의_2_4배_피해를_준다()
        {
            var vampire = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 300);
            var target = TestActorFactory.Create(name: "고블린", maxHp: 1000, defense: 0);

            var result = new SkillAction(DefaultSkills.BloodExplosion, vampire, target).Execute();

            Assert.That(vampire.Health.CurrentHp, Is.EqualTo(70));  // 100 * 0.3 = 30
            Assert.That(result.DamageDealt, Is.EqualTo(720));       // 300 * 2.4 = 720 (자해는 제외)
            Assert.That(target.Health.CurrentHp, Is.EqualTo(280));
        }

        [Test]
        public void Thirster는_피해를_주고_체력이_부족할_때만_회복하며_최대체력을_넘지_않는다()
        {
            var target = TestActorFactory.Create(name: "고블린", maxHp: 1000, defense: 0);

            // 체력이 부족한 경우: 100 * 0.1 = 10 피해, 100 * 0.08 = 8 회복
            var hurt = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 100);
            hurt.ApplyTrueDamage(30); // 체력 70
            var result = new SkillAction(DefaultSkills.Thirster, hurt, target).Execute();

            Assert.That(result.DamageDealt, Is.EqualTo(10));
            Assert.That(hurt.Health.CurrentHp, Is.EqualTo(78));

            // 체력이 가득 찬 경우: 회복은 일어나지 않음
            var full = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 100);
            new SkillAction(DefaultSkills.Thirster, full, target).Execute();

            Assert.That(full.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void OverFlow는_공격력을_영구적으로_1_올린다()
        {
            var vampire = TestActorFactory.Create(name: "흡혈귀", attack: 10);
            var target = TestActorFactory.Create(name: "고블린");

            new SkillAction(DefaultSkills.OverFlow, vampire, target).Execute();

            Assert.That(vampire.Stats.GetValue(StatType.Attack), Is.EqualTo(11));
        }

        [Test]
        public void BloodDiamond는_처음에는_혈석을_부여하고_다시_쓰면_스택을_쌓아_강화해야_한다()
        {
            var vampire = TestActorFactory.Create(name: "흡혈귀");
            var target = TestActorFactory.Create(name: "고블린");

            new SkillAction(DefaultSkills.BloodDiamond, vampire, target).Execute();
            Assert.That(vampire.StatusEffects.GetStacks(StatusEffectType.BloodDiamond), Is.EqualTo(1));

            new SkillAction(DefaultSkills.BloodDiamond, vampire, target).Execute();
            Assert.That(vampire.StatusEffects.GetStacks(StatusEffectType.BloodDiamond), Is.EqualTo(2));
        }

        [Test]
        public void BloodDiamond를_쓴_뒤_매_턴_종료에_상대에게_늘어나는_피해가_들어가야_한다()
        {
            var vampire = TestActorFactory.Create(name: "흡혈귀", role: ActorRole.Player, maxHp: 1000, attack: 10);
            var monster = TestActorFactory.Create(name: "고블린", role: ActorRole.Enemy, maxHp: 1000, attack: 1);
            var turnManager = new TurnManager(vampire, monster, new ActionPipeline());

            turnManager.ExecuteRound(new SkillAction(DefaultSkills.BloodDiamond, vampire, monster)); // 턴 종료 혈석 1 피해
            turnManager.ExecuteRound(new SkillAction(DefaultSkills.BatFire, vampire, monster));      // 10 피해, 턴 종료 혈석 2 피해

            Assert.That(monster.Health.CurrentHp, Is.EqualTo(1000 - 1 - 10 - 2));
        }

        [Test]
        public void 흡혈귀_스킬_풀은_원본의_5종이어야_한다()
        {
            Assert.That(DefaultSkills.VampireSkills.Select(s => s.Name),
                Is.EqualTo(new[] { "Bat_Fire", "Blood_Explosion", "Thirster", "OverFlow", "Blood_Diamond" }));
        }
    }
}