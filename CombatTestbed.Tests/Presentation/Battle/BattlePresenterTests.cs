using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Battle
{
    [TestFixture]
    public class BattlePresenterTests
    {
        private static readonly SkillDefinition Strike = Skill("strike", SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f));

        private static SkillDefinition Skill(string id, params SkillEffectData[] effects)
        {
            return new SkillDefinition(id, id, effects);
        }

        private sealed class Fixture
        {
            public CampaignManager Campaign;
            public FakeBattleView View = new FakeBattleView();
            public FakeChoiceView ChoiceView = new FakeChoiceView();
            public ChoicePresenter Choices;
            public BattlePresenter Presenter;
        }

        // 전투 단계 하나짜리 챕터로 캠페인을 만들고 Presenter를 연결한 뒤 전투를 시작한 상태
        private static Fixture StartBattle(
            int monsterHp, int monsterAttack, SkillDefinition[] skills = null, int heroAttack = 50)
        {
            var monster = new ActorDefinition("slime", "slime", ActorRole.Enemy, monsterHp, monsterAttack);
            var chapter = new ChapterDefinition(
                "c1", "c1", new[] { new StageDefinition(StageType.Battle) }, new[] { monster });
            var hero = new ActorDefinition("hero", "용사", ActorRole.Player, 100, heroAttack);

            var fixture = new Fixture();
            fixture.Campaign = new CampaignManager(
                new[] { chapter }, hero, skills ?? new[] { Strike }, new SkillDefinition[0], new SequenceRandomSource(0));
            fixture.Choices = new ChoicePresenter(fixture.ChoiceView);
            fixture.Presenter = new BattlePresenter(fixture.View, fixture.Choices);
            fixture.Presenter.Bind(fixture.Campaign);
            fixture.Campaign.Start();
            return fixture;
        }

        private static void Use(Fixture fixture, string skillId)
        {
            fixture.Presenter.HandleChoice(new ActionChoice(skillId, skillId));
        }

        [Test]
        public void 전투가_시작되면_로그를_지우고_헤더와_양쪽_액터와_스킬_선택지를_보여야_한다()
        {
            var f = StartBattle(monsterHp: 30, monsterAttack: 1);

            Assert.That(f.View.ClearLogCount, Is.EqualTo(1));
            Assert.That(f.View.Header, Is.EqualTo("챕터 1 : c1 (stage 1) - 라운드 1"));
            Assert.That(f.View.Actors[ActorSide.Player].Name, Is.EqualTo("용사"));
            Assert.That(f.View.Actors[ActorSide.Player].CurrentHp, Is.EqualTo(100));
            Assert.That(f.View.Actors[ActorSide.Player].Attack, Is.EqualTo(50));
            Assert.That(f.View.Actors[ActorSide.Enemy].Name, Is.EqualTo("slime"));
            Assert.That(f.View.Actors[ActorSide.Enemy].CurrentHp, Is.EqualTo(30));
            Assert.That(f.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "strike" }));
            Assert.That(f.ChoiceView.Shown[0].Label, Is.EqualTo("strike"));
        }

        [Test]
        public void 스킬을_골라_적을_쓰러뜨리면_행동_문구_다음에_쓰러짐_문구가_나오고_체력이_갱신되어야_한다()
        {
            var f = StartBattle(monsterHp: 30, monsterAttack: 1);

            Use(f, "strike");

            Assert.That(f.View.Log, Is.EqualTo(new[]
            {
                "용사이(가) slime에게 strike을(를) 사용해 30의 피해를 입혔습니다.",
                "slime이(가) 쓰러졌다."
            }));
            Assert.That(f.View.Actors[ActorSide.Enemy].CurrentHp, Is.EqualTo(0));
            Assert.That(f.Campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
        }

        [Test]
        public void 상태이상이_부여되면_행동_문구_뒤에_기록하고_턴_종료_틱_피해는_상태이상_문구로_기록해야_한다()
        {
            var sting = Skill("sting",
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.2f),
                SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Poison, 3));
            var f = StartBattle(monsterHp: 1000, monsterAttack: 1, skills: new[] { sting });

            Use(f, "sting");

            Assert.That(f.View.Log, Is.EqualTo(new[]
            {
                "용사이(가) slime에게 sting을(를) 사용해 10의 피해를 입혔습니다.",
                "slime에게 중독 3이(가) 걸렸다.",
                "slime이(가) 용사에게 일반 공격으로 1의 피해를 입혔습니다.",
                "slime이(가) 상태이상으로 3의 피해를 입었다."
            }));
            Assert.That(f.View.Actors[ActorSide.Enemy].Statuses, Is.EqualTo(new[] { "중독 2" }));
            Assert.That(f.View.Actors[ActorSide.Enemy].CurrentHp, Is.EqualTo(1000 - 10 - 3));
        }

        [Test]
        public void 마비된_적은_행동하지_못한다는_문구를_기록하고_마비가_풀리면_상태_목록에서_사라져야_한다()
        {
            var curse = Skill("curse", SkillEffectData.ApplyStatus(EffectTarget.Opponent, StatusEffectType.Paralysis, 1));
            var f = StartBattle(monsterHp: 1000, monsterAttack: 1, skills: new[] { curse });

            Use(f, "curse");

            Assert.That(f.View.Log, Is.EqualTo(new[]
            {
                "용사이(가) curse을(를) 사용했습니다.",
                "slime에게 마비 1이(가) 걸렸다.",
                "slime은(는) 마비 상태라서 행동할 수 없다!"
            }));
            Assert.That(f.View.Actors[ActorSide.Enemy].Statuses, Is.Empty); // 턴 종료에 마비 소진
        }

        [Test]
        public void 행동_중의_회복은_행동_문구_뒤에_기록되어야_한다()
        {
            var drain = Skill("drain",
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 0.2f),
                SkillEffectData.Heal(EffectTarget.Self, StatType.Attack, 0.2f));
            var f = StartBattle(monsterHp: 1000, monsterAttack: 1, skills: new[] { drain });
            f.Campaign.Player.ApplyTrueDamage(50); // 체력 50

            Use(f, "drain");

            Assert.That(f.View.Log.Count, Is.EqualTo(3));
            Assert.That(f.View.Log[0], Does.Contain("drain"));
            Assert.That(f.View.Log[1], Is.EqualTo("용사이(가) 10 회복했다."));
            Assert.That(f.View.Actors[ActorSide.Player].CurrentHp, Is.EqualTo(50 + 10 - 1)); // 회복 후 적의 반격 1
        }

        [Test]
        public void 라운드가_진행되면_헤더의_라운드_번호가_갱신되어야_한다()
        {
            var f = StartBattle(monsterHp: 100000, monsterAttack: 1);

            Use(f, "strike");
            Assert.That(f.View.Header, Is.EqualTo("챕터 1 : c1 (stage 1) - 라운드 1"));

            Use(f, "strike");
            Assert.That(f.View.Header, Is.EqualTo("챕터 1 : c1 (stage 1) - 라운드 2"));
        }

        [Test]
        public void 전투가_끝나면_그_전투의_이벤트_구독이_해제되어야_한다()
        {
            var f = StartBattle(monsterHp: 30, monsterAttack: 1);
            var battle = f.Campaign.CurrentBattle;
            Use(f, "strike"); // 전투 종료
            int showCountAfterBattle = f.View.ShowActorCount;
            int logCountAfterBattle = f.View.Log.Count;

            battle.Player.ApplyTrueDamage(1); // 끝난 전투의 객체에 변화가 생겨도

            Assert.That(f.View.ShowActorCount, Is.EqualTo(showCountAfterBattle)); // 화면은 움직이지 않음
            Assert.That(f.View.Log.Count, Is.EqualTo(logCountAfterBattle));
        }

        [Test]
        public void 알_수_없는_선택지와_연결_전_선택은_무시되어야_한다()
        {
            var f = StartBattle(monsterHp: 1000, monsterAttack: 1);
            var unbound = new BattlePresenter(new FakeBattleView(), new ChoicePresenter(new FakeChoiceView()));

            Assert.DoesNotThrow(() => f.Presenter.HandleChoice(new ActionChoice("unknown", "?")));
            Assert.DoesNotThrow(() => unbound.HandleChoice(new ActionChoice("strike", "strike")));

            Assert.That(f.View.Log, Is.Empty);
            Assert.That(f.Campaign.Phase, Is.EqualTo(CampaignPhase.Battle));
        }

        [Test]
        public void 연결_대상을_바꾸거나_Dispose하면_이전_캠페인은_더_이상_화면을_움직이지_않아야_한다()
        {
            var monster = new ActorDefinition("slime", "slime", ActorRole.Enemy, 30, 1);
            Func<CampaignManager> newCampaign = () => new CampaignManager(
                new[] { new ChapterDefinition("c1", "c1", new[] { new StageDefinition(StageType.Battle) }, new[] { monster }) },
                new ActorDefinition("hero", "용사", ActorRole.Player, 100, 50),
                new[] { Strike }, new SkillDefinition[0], new SequenceRandomSource(0));
            var view = new FakeBattleView();
            var presenter = new BattlePresenter(view, new ChoicePresenter(new FakeChoiceView()));
            var oldCampaign = newCampaign();
            var currentCampaign = newCampaign();

            presenter.Bind(oldCampaign);
            presenter.Bind(currentCampaign);
            oldCampaign.Start();
            Assert.That(view.ClearLogCount, Is.EqualTo(0)); // 연결이 해제된 캠페인

            currentCampaign.Start();
            Assert.That(view.ClearLogCount, Is.EqualTo(1));

            var disposedView = new FakeBattleView();
            var disposed = new BattlePresenter(disposedView, new ChoicePresenter(new FakeChoiceView()));
            var campaignAfterDispose = newCampaign();
            disposed.Bind(campaignAfterDispose);
            disposed.Dispose();
            disposed.Dispose(); // 중복 호출 안전
            campaignAfterDispose.Start();

            Assert.That(disposedView.ClearLogCount, Is.EqualTo(0));
            Assert.Throws<ObjectDisposedException>(() => disposed.Bind(newCampaign()));
        }

        [Test]
        public void 잘못된_입력으로는_Presenter를_만들거나_연결할_수_없어야_한다()
        {
            var view = new FakeBattleView();
            var choices = new ChoicePresenter(new FakeChoiceView());
            var presenter = new BattlePresenter(view, choices);

            Assert.Throws<ArgumentNullException>(() => new BattlePresenter(null, choices));
            Assert.Throws<ArgumentNullException>(() => new BattlePresenter(view, null));
            Assert.Throws<ArgumentNullException>(() => presenter.Bind(null));
        }
    }
}