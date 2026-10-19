using System;
using NUnit.Framework;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Skills
{
    [TestFixture]
    public class SkillActionTests
    {
        private static SkillDefinition Skill(string name, params SkillEffectData[] effects)
        {
            return new SkillDefinition("test_skill", name, effects);
        }

        [Test]
        public void 피해_스킬은_계수를_적용하고_방어력을_차감한_실피해를_결과에_담아야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 100);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 1000, defense: 10);
            var action = new SkillAction(
                Skill("강타", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.4f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DamageDealt, Is.EqualTo(230)); // 100 * 2.4 = 240, 방어 10 차감
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(770));
        }

        [Test]
        public void 자해_피해와_상대_피해를_함께_가진_스킬은_자신에게_방어무시_피해를_주고_결과에는_상대_피해만_담아야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 10);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100, defense: 0);
            var action = new SkillAction(
                Skill("블러드 폭발",
                    SkillEffectData.Damage(EffectTarget.Self, StatType.MaxHp, 0.3f, DamageType.True),
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.4f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(attacker.Health.CurrentHp, Is.EqualTo(70));  // 최대 체력 100의 30% = 30
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(76));  // 10 * 2.4 = 24
            Assert.That(result.DamageDealt, Is.EqualTo(24));         // 자해 30은 제외
        }

        [Test]
        public void 피해와_회복을_함께_가진_스킬은_상대에게_피해를_주고_자신을_회복해야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 100);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100, defense: 0);
            attacker.ApplyTrueDamage(30); // 체력 70
            var action = new SkillAction(
                Skill("흡혈",
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.1f),
                    SkillEffectData.Heal(EffectTarget.Self, StatType.Attack, 0.08f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(defender.Health.CurrentHp, Is.EqualTo(90)); // 100 * 0.1 = 10
            Assert.That(attacker.Health.CurrentHp, Is.EqualTo(78)); // 70 + 100 * 0.08 = 78
            Assert.That(result.DamageDealt, Is.EqualTo(10));
        }

        [Test]
        public void 피해와_상태이상을_함께_가진_스킬은_상대에게_피해를_주고_상태이상을_부여해야_한다()
        {
            var attacker = TestActorFactory.Create(name: "사냥꾼", maxHp: 100, attack: 20);
            var defender = TestActorFactory.Create(name: "용사", maxHp: 100, defense: 0);
            var action = new SkillAction(
                Skill("독 화살",
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.8f),
                    SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Poison, 1)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(defender.Health.CurrentHp, Is.EqualTo(84)); // 20 * 0.8 = 16
            Assert.That(defender.StatusEffects.GetStacks(StatusEffectType.Poison), Is.EqualTo(1));
            Assert.That(result.DamageDealt, Is.EqualTo(16));
        }

        [Test]
        public void 같은_피해_효과를_두번_가진_스킬은_연속으로_두번_피해를_주어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "사냥꾼", maxHp: 100, attack: 20);
            var defender = TestActorFactory.Create(name: "용사", maxHp: 100, defense: 0);
            int hitCount = 0;
            defender.DamageTaken += (a, amount) => hitCount++;
            var action = new SkillAction(
                Skill("연속 화살",
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.5f),
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.5f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(hitCount, Is.EqualTo(2));
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(80)); // 10 + 10
            Assert.That(result.DamageDealt, Is.EqualTo(20));
        }

        [Test]
        public void 고정값_스탯_증가_스킬은_공격력을_영구적으로_올려야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", maxHp: 100, attack: 10);
            var defender = TestActorFactory.Create(name: "고블린");
            var action = new SkillAction(
                Skill("오버플로", SkillEffectData.StatGain(EffectTarget.Self, StatType.Attack, 0f, flatValue: 1)),
                attacker, defender);

            action.Execute();

            Assert.That(attacker.Stats.GetValue(StatType.Attack), Is.EqualTo(11));
        }

        [Test]
        public void 최대체력_증가_스킬은_최대체력과_현재체력을_함께_올려야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", maxHp: 100);
            var defender = TestActorFactory.Create(name: "고블린");
            attacker.ApplyTrueDamage(30); // 체력 70
            var action = new SkillAction(
                Skill("활력", SkillEffectData.StatGain(EffectTarget.Self, StatType.MaxHp, 0f, flatValue: 20)),
                attacker, defender);

            action.Execute();

            Assert.That(attacker.Health.MaxHp, Is.EqualTo(120));
            Assert.That(attacker.Health.CurrentHp, Is.EqualTo(90)); // 70 + 20
        }

        [Test]
        public void 비율_스탯_증가_스킬은_현재_스탯에_계수를_곱한_만큼_올려야_한다()
        {
            var attacker = TestActorFactory.Create(name: "늑대인간", maxHp: 100, attack: 30);
            var defender = TestActorFactory.Create(name: "용사");
            var action = new SkillAction(
                Skill("달빛 강화", SkillEffectData.StatGain(EffectTarget.Self, StatType.Attack, 0.5f)),
                attacker, defender);

            action.Execute();

            Assert.That(attacker.Stats.GetValue(StatType.Attack), Is.EqualTo(45)); // 30 + 30 * 0.5
        }

        [Test]
        public void 시전자가_마비면_CanExecute는_false이고_직접_Execute를_호출하면_예외가_발생해야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", attack: 20);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100);
            attacker.StatusEffects.ApplyEffect(attacker, new ParalysisEffect(1));
            var action = new SkillAction(
                Skill("강타", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)),
                attacker, defender);

            Assert.That(action.CanExecute(), Is.False);
            Assert.Throws<InvalidOperationException>(() => action.Execute());
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void 파이프라인을_거치면_스킬이_성공_결과로_실행되어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", attack: 20);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100, defense: 5);
            var action = new SkillAction(
                Skill("강타", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)),
                attacker, defender);

            ActionResult result = new ActionPipeline().Execute(action);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DamageDealt, Is.EqualTo(15)); // 20 - 방어 5
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(85));
        }

        [Test]
        public void 파이프라인을_거치면_마비된_시전자의_스킬은_실패_결과여야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", attack: 20);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100);
            attacker.StatusEffects.ApplyEffect(attacker, new ParalysisEffect(1));
            var action = new SkillAction(
                Skill("강타", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)),
                attacker, defender);

            ActionResult result = new ActionPipeline().Execute(action);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DamageDealt, Is.EqualTo(0));
            Assert.That(defender.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void 스킬_행동은_정의_시전자_대상이_null이면_생성할_수_없어야_한다()
        {
            var skill = Skill("강타", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f));
            var actor = TestActorFactory.Create();

            Assert.Throws<ArgumentNullException>(() => new SkillAction(null, actor, actor));
            Assert.Throws<ArgumentNullException>(() => new SkillAction(skill, null, actor));
            Assert.Throws<ArgumentNullException>(() => new SkillAction(skill, actor, null));
        }

        [Test]
        public void 결과_메시지에는_스킬_이름과_상대에게_준_피해가_포함되어야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", attack: 10);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 100, defense: 0);
            var action = new SkillAction(
                Skill("블러드 폭발", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.4f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(result.Message, Does.Contain("블러드 폭발"));
            Assert.That(result.Message, Does.Contain("24"));
        }

        [Test]
        public void 첫_효과로_상대가_사망하면_이어지는_피해_효과는_추가_피해와_이벤트를_만들지_않아야_한다()
        {
            var attacker = TestActorFactory.Create(name: "흡혈귀", attack: 10);
            var defender = TestActorFactory.Create(name: "고블린", maxHp: 10, defense: 0);
            int damageCount = 0;
            int diedCount = 0;
            defender.DamageTaken += (a, amount) => damageCount++;
            defender.Died += a => diedCount++;
            var action = new SkillAction(
                Skill("연속 공격",
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f),
                    SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)),
                attacker, defender);

            ActionResult result = action.Execute();

            Assert.That(result.DamageDealt, Is.EqualTo(10)); // 두 번째 효과의 피해는 0
            Assert.That(damageCount, Is.EqualTo(1));
            Assert.That(diedCount, Is.EqualTo(1));
        }
    }
}