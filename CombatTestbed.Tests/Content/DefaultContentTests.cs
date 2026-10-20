using NUnit.Framework;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Tests.Content
{
    [TestFixture]
    public class DefaultContentTests
    {
        [Test]
        public void 기본_액터_11종의_스탯은_원본_게임과_같아야_한다()
        {
            // (정의, 최대 체력, 공격력) - 원본 각 클래스의 필드 값
            var cases = new[]
            {
                (def: DefaultActors.Vampire,    maxHp: 100, attack: 300),
                (def: DefaultActors.Farmer,     maxHp: 20,  attack: 2),
                (def: DefaultActors.Hunter,     maxHp: 25,  attack: 5),
                (def: DefaultActors.Wolf,       maxHp: 30,  attack: 3),
                (def: DefaultActors.Knight,     maxHp: 80,  attack: 17),
                (def: DefaultActors.Werewolf,   maxHp: 60,  attack: 15),
                (def: DefaultActors.Paladin,    maxHp: 150, attack: 20),
                (def: DefaultActors.Lycan,      maxHp: 200, attack: 20),
                (def: DefaultActors.Priest,     maxHp: 50,  attack: 10),
                (def: DefaultActors.VanHelsing, maxHp: 150, attack: 20),
                (def: DefaultActors.WereFenrir, maxHp: 350, attack: 30),
            };

            foreach (var c in cases)
            {
                string label = $"액터: {c.def.Id}";
                Assert.That(c.def.MaxHp, Is.EqualTo(c.maxHp), label);
                Assert.That(c.def.Attack, Is.EqualTo(c.attack), label);
                Assert.That(c.def.Defense, Is.EqualTo(0), label); // 방어력은 원본에 없던 개념
            }
        }

        [Test]
        public void 몬스터는_원본_보상을_가지고_보스와_플레이어는_보상이_없으며_보스만_스킬을_가져야_한다()
        {
            Assert.That(DefaultActors.Farmer.Reward, Is.SameAs(DefaultRewards.Farmer));
            Assert.That(DefaultActors.Hunter.Reward, Is.SameAs(DefaultRewards.Hunter));
            Assert.That(DefaultActors.Wolf.Reward, Is.SameAs(DefaultRewards.Wolf));
            Assert.That(DefaultActors.Knight.Reward, Is.SameAs(DefaultRewards.Knight));
            Assert.That(DefaultActors.Werewolf.Reward, Is.SameAs(DefaultRewards.Werewolf));
            Assert.That(DefaultActors.Paladin.Reward, Is.SameAs(DefaultRewards.Paladin));
            Assert.That(DefaultActors.Lycan.Reward, Is.SameAs(DefaultRewards.Lycan));

            Assert.That(DefaultActors.Vampire.Reward, Is.Null);
            Assert.That(DefaultActors.Priest.Reward, Is.Null);
            Assert.That(DefaultActors.VanHelsing.Reward, Is.Null);
            Assert.That(DefaultActors.WereFenrir.Reward, Is.Null);

            Assert.That(DefaultActors.Farmer.BehaviorSkills.Count, Is.EqualTo(0));
            Assert.That(DefaultActors.Priest.BehaviorSkills.Count, Is.EqualTo(3));
            Assert.That(DefaultActors.VanHelsing.BehaviorSkills.Count, Is.EqualTo(3));
            Assert.That(DefaultActors.WereFenrir.BehaviorSkills.Count, Is.EqualTo(3));
        }

        [Test]
        public void 성직자의_스킬_구성은_원본과_같아야_한다()
        {
            var skills = DefaultActors.Priest.BehaviorSkills;

            Assert.That(skills[0].Name, Is.EqualTo("주문 낭독")); // READSPELL: 마비 1
            Assert.That(skills[0].Effects.Count, Is.EqualTo(1));
            Assert.That(skills[0].Effects[0].Kind, Is.EqualTo(SkillEffectKind.ApplyStatus));
            Assert.That(skills[0].Effects[0].StatusType, Is.EqualTo(StatusEffectType.Paralysis));
            Assert.That(skills[0].Effects[0].Stacks, Is.EqualTo(1));

            Assert.That(skills[1].Name, Is.EqualTo("십자가")); // CROSS: 공격력 1배 피해
            Assert.That(skills[1].Effects[0].Kind, Is.EqualTo(SkillEffectKind.Damage));
            Assert.That(skills[1].Effects[0].Coefficient, Is.EqualTo(1.0f));

            Assert.That(skills[2].Name, Is.EqualTo("기도")); // PRAY: 자신 10 회복
            Assert.That(skills[2].Effects[0].Kind, Is.EqualTo(SkillEffectKind.Heal));
            Assert.That(skills[2].Effects[0].Target, Is.EqualTo(EffectTarget.Self));
            Assert.That(skills[2].Effects[0].FlatValue, Is.EqualTo(10));
        }

        [Test]
        public void 반_헬싱의_스킬_구성은_원본과_같아야_한다()
        {
            var skills = DefaultActors.VanHelsing.BehaviorSkills;

            Assert.That(skills[0].Name, Is.EqualTo("은 화살")); // SILVERARROW: 마비 1
            Assert.That(skills[0].Effects[0].StatusType, Is.EqualTo(StatusEffectType.Paralysis));

            Assert.That(skills[1].Name, Is.EqualTo("연속 화살")); // CONTINUOUSARROW: 0.5배 x 2
            Assert.That(skills[1].Effects.Count, Is.EqualTo(2));
            Assert.That(skills[1].Effects[0].Coefficient, Is.EqualTo(0.5f));
            Assert.That(skills[1].Effects[1].Coefficient, Is.EqualTo(0.5f));

            Assert.That(skills[2].Name, Is.EqualTo("독 화살")); // POISONARROW: 0.8배 + 중독 1
            Assert.That(skills[2].Effects.Count, Is.EqualTo(2));
            Assert.That(skills[2].Effects[0].Coefficient, Is.EqualTo(0.8f));
            Assert.That(skills[2].Effects[1].StatusType, Is.EqualTo(StatusEffectType.Poison));
            Assert.That(skills[2].Effects[1].Stacks, Is.EqualTo(1));
        }

        [Test]
        public void 베어펜리르의_스킬_구성은_원본과_같아야_한다()
        {
            var skills = DefaultActors.WereFenrir.BehaviorSkills;

            Assert.That(skills[0].Name, Is.EqualTo("달빛 강화")); // MOONHOPER: 공격력 50% 증가
            Assert.That(skills[0].Effects[0].Kind, Is.EqualTo(SkillEffectKind.StatGain));
            Assert.That(skills[0].Effects[0].Coefficient, Is.EqualTo(0.5f));

            Assert.That(skills[1].Name, Is.EqualTo("할퀴기")); // SCRATCH: 0.7배 + 출혈 10
            Assert.That(skills[1].Effects[0].Coefficient, Is.EqualTo(0.7f));
            Assert.That(skills[1].Effects[1].StatusType, Is.EqualTo(StatusEffectType.Bleed));
            Assert.That(skills[1].Effects[1].Stacks, Is.EqualTo(10));

            Assert.That(skills[2].Name, Is.EqualTo("물기")); // BITE: 0.7배
            Assert.That(skills[2].Effects[0].Coefficient, Is.EqualTo(0.7f));
        }

        [Test]
        public void 베어펜리르가_달빛_강화_뒤_물기를_쓰면_원본과_같은_피해가_들어가야_한다()
        {
            var boss = ActorFactory.Create(DefaultActors.WereFenrir);
            var player = TestPlayer();

            new SkillAction(DefaultSkills.WereFenrirMoonLeap, boss, player).Execute();
            var result = new SkillAction(DefaultSkills.WereFenrirBite, boss, player).Execute();

            Assert.That(boss.Stats.GetValue(StatType.Attack), Is.EqualTo(45)); // 30 * 1.5
            Assert.That(result.DamageDealt, Is.EqualTo(31));                   // (int)(45 * 0.7) = 31
            Assert.That(player.Health.CurrentHp, Is.EqualTo(969));
        }

        private static CombatTestbed.Core.Domain.Actors.BattleActor TestPlayer()
        {
            return CombatTestbed.Tests.TestUtils.TestActorFactory.Create(name: "용사", maxHp: 1000, defense: 0);
        }
    }
}