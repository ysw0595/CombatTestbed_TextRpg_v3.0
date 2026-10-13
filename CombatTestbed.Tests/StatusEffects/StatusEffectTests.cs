using NUnit.Framework;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Tests.Domain.StatusEffects
{
    [TestFixture]
    public class StatusEffectTests
    {
        private BattleActor CreateTestTarget(int maxHp, int defense)
        {
            StatSheet stats = new StatSheet();
            stats.SetStat(StatType.MaxHp, maxHp);
            stats.SetStat(StatType.Defense, defense);

            return new BattleActor("test_dummy", "훈련용 허수아비", ActorRole.Enemy, stats);
        }

        [Test]
        public void 중독_적용시_턴종료때_방어무시_피해를_입히고_스택이_1_감소해야_한다()
        {
            // 1. Arrange (준비)
            // 체력 100, 방어력 50인 허수아비 생성
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 50);

            // 3스택짜리 독 생성 후 허수아비 주머니에 부여
            PoisonEffect poison = new PoisonEffect(initialStacks: 3);
            target.StatusEffects.ApplyEffect(target, poison);

            // 2. Act (실행)
            // 턴 종료 신호 발송 (독 틱 데미지 발동)
            target.EndTurn();

            // 3. Assert (검증)
            // 방어력이 50이나 되지만, 독은 트루뎀이므로 3 피해가 고스란히 들어가 체력이 97이어야 함
            Assert.That(target.Health.CurrentHp, Is.EqualTo(97));
            // 턴이 끝났으니 중독 스택은 3에서 1 감소한 2여야 함
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Poison), Is.EqualTo(2));
        }

        [Test]
        public void 중독_스택이_0이_되면_주머니에서_자동으로_안전하게_제거되어야_한다()
        {
            // 1. Arrange (준비)
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 0);
            PoisonEffect poison = new PoisonEffect(initialStacks: 1);
            target.StatusEffects.ApplyEffect(target, poison);

            // 2. Act (실행)
            // 1스택인 상태에서 턴 종료 -> 1 피해를 주고 스택이 0이 됨 -> 소멸 판정
            target.EndTurn();

            // 3. Assert (검증)
            // 허수아비의 주머니에 더 이상 독이 남아있지 않아야 함
            Assert.That(target.StatusEffects.HasEffect(StatusEffectType.Poison), Is.False);
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Poison), Is.EqualTo(0));
        }

        [Test]
        public void 동일한_상태이상을_추가_부여하면_스택이_정상적으로_합산되어야_한다()
        {
            // 1. Arrange
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 0);

            // 3스택 부여
            PoisonEffect poison1 = new PoisonEffect(initialStacks: 3);
            target.StatusEffects.ApplyEffect(target, poison1);

            // 2. Act
            // 동일한 독 2스택 추가 부여
            PoisonEffect poison2 = new PoisonEffect(initialStacks: 2);
            target.StatusEffects.ApplyEffect(target, poison2);

            // 3. Assert
            // 3 + 2 = 5스택이어야 하고, 효과 인스턴스는 1개만 유지되어야 함
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Poison), Is.EqualTo(5));
        }

        [Test]
        public void 출혈_적용시_턴종료때_스택만큼_방어무시_피해를_입히고_스택은_감소하지_않아야_한다()
        {
            // 1. Arrange (체력 100, 방어력 30, 출혈 4스택)
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 30);
            target.StatusEffects.ApplyEffect(target, new BleedEffect(initialStacks: 4));

            // 2. Act & Assert: 턴 시작에는 피해가 없어야 함
            target.StartTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(100));

            // 3. Act & Assert: 1번째 턴 종료 -> 4 피해, 스택 유지
            target.EndTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(96));
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(4));

            // 4. Act & Assert: 2번째 턴 종료 -> 또 4 피해, 스택은 여전히 4
            target.EndTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(92));
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(4));
        }

        [Test]
        public void 출혈은_외부_정화_전까지_영구지속되며_정화시_즉시_제거되어_피해가_멈춰야_한다()
        {
            // 1. Arrange (체력 100, 출혈 5스택)
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 30);
            target.StatusEffects.ApplyEffect(target, new BleedEffect(initialStacks: 5));

            // 2. 1턴 종료 (5 피해 -> 95), 스택 유지
            target.EndTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(95));
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(5));

            // 3. 2턴 종료 (5 피해 -> 90), 스택은 여전히 영구 유지
            target.EndTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(90));
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(5));

            // 4. 정화 스킬(RemoveEffect) 발동
            bool isRemoved = target.StatusEffects.RemoveEffect(target, StatusEffectType.Bleed);

            // 5. 주머니에서 완전히 비워졌는지 확인
            Assert.That(isRemoved, Is.True);
            Assert.That(target.StatusEffects.HasEffect(StatusEffectType.Bleed), Is.False);
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Bleed), Is.EqualTo(0));

            // 6. 3턴 종료 (출혈이 지워졌으므로 피해 없음 -> 90 유지)
            target.EndTurn();
            Assert.That(target.Health.CurrentHp, Is.EqualTo(90));
        }

        [Test]
        public void 마비_1스택_부여시_행동불가상태가_되고_턴종료때_스택이_소모되어_다음턴에_풀려야_한다()
        {
            // 1. Arrange (체력 100 허수아비에 마비 1스택 부여)
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 0);
            ParalysisEffect paralysis = new ParalysisEffect(initialStacks: 1);
            target.StatusEffects.ApplyEffect(target, paralysis);

            // 2. Assert: 현재 턴에는 행동 불가여야 함
            Assert.That(target.CanAct, Is.False);
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Paralysis), Is.EqualTo(1));

            // 3. Act: 턴 종료 (1턴 행동 차단 완료 -> 스택 1 소모 -> 0스택 만료 정리)
            target.EndTurn();

            // 4. Assert: 마비가 풀려 행동 가능 상태여야 함
            Assert.That(target.CanAct, Is.True);
            Assert.That(target.StatusEffects.HasEffect(StatusEffectType.Paralysis), Is.False);
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Paralysis), Is.EqualTo(0));
        }

        [Test]
        public void 마비_2스택_부여시_한턴이_끝나도_1스택이_남아_여전히_행동불가여야_한다()
        {
            // 1. Arrange (마비 2스택 부여)
            BattleActor target = CreateTestTarget(maxHp: 100, defense: 0);
            ParalysisEffect paralysis = new ParalysisEffect(initialStacks: 2);
            target.StatusEffects.ApplyEffect(target, paralysis);

            // 2. Act: 첫 번째 턴 종료
            target.EndTurn();

            // 3. Assert: 1스택이 남아있으므로 여전히 행동 불가
            Assert.That(target.CanAct, Is.False);
            Assert.That(target.StatusEffects.GetStacks(StatusEffectType.Paralysis), Is.EqualTo(1));

            // 4. Act: 두 번째 턴 종료
            target.EndTurn();

            // 5. Assert: 2턴을 모두 소모했으므로 완전히 풀림
            Assert.That(target.CanAct, Is.True);
            Assert.That(target.StatusEffects.HasEffect(StatusEffectType.Paralysis), Is.False);
        }
    }
}