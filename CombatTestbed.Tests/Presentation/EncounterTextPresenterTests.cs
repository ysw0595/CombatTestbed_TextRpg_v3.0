using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Screens;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation
{
    [TestFixture]
    public class EncounterTextPresenterTests
    {
        private static RewardDefinition RewardOf(params SkillEffectData[] effects)
        {
            return new RewardDefinition("r", effects);
        }

        [Test]
        public void 등장_문구가_있으면_전투_시작_때_로그의_첫_줄로_보여야_한다()
        {
            var texts = new FakeEncounterTextProvider();
            texts.Appear["slime"] = "슬라임이 나타났다.";
            var view = new FakeBattleView();
            var presenter = new BattlePresenter(view, new ChoicePresenter(new FakeChoiceView()), texts);
            var campaign = TestCampaigns.SingleStage(
                new StageDefinition(StageType.Battle), TestCampaigns.Monster("slime", 30, 1));
            presenter.Bind(campaign);

            campaign.Start();

            Assert.That(view.Log, Is.EqualTo(new[] { "슬라임이 나타났다." }));
        }

        [Test]
        public void 등장_문구가_없으면_로그에_아무것도_추가하지_않아야_한다()
        {
            var texts = new FakeEncounterTextProvider(); // 아무 문구도 넣지 않음
            var view = new FakeBattleView();
            var presenter = new BattlePresenter(view, new ChoicePresenter(new FakeChoiceView()), texts);
            var campaign = TestCampaigns.SingleStage(
                new StageDefinition(StageType.Battle), TestCampaigns.Monster("slime", 30, 1));
            presenter.Bind(campaign);

            campaign.Start();

            Assert.That(view.Log, Is.Empty);
        }

        [Test]
        public void 처치_문구는_보상_문구_앞에_빈_줄을_사이에_두고_나와야_한다()
        {
            var texts = new FakeEncounterTextProvider();
            texts.Slain["slime"] = "슬라임은 쓰러졌다.";
            var view = new FakeStageResultView();
            var presenter = new StageResultPresenter(view, new ChoicePresenter(new FakeChoiceView()), texts);
            var slime = TestCampaigns.Monster("slime", 30, 1,
                RewardOf(SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.MaxHp, 5)));
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Battle), slime);
            presenter.Bind(campaign);
            campaign.Start();

            campaign.UseSkill(TestCampaigns.Strike);

            Assert.That(view.Lines, Is.EqualTo(new[] { "슬라임은 쓰러졌다.", "", "최대 체력이 5 증가했다." }));
        }

        [Test]
        public void 보상이_없으면_처치_문구만_나오고_빈_줄은_붙지_않아야_한다()
        {
            var texts = new FakeEncounterTextProvider();
            texts.Slain["slime"] = "슬라임은 쓰러졌다.";
            var view = new FakeStageResultView();
            var presenter = new StageResultPresenter(view, new ChoicePresenter(new FakeChoiceView()), texts);
            var campaign = TestCampaigns.SingleStage(
                new StageDefinition(StageType.Battle), TestCampaigns.Monster("slime", 30, 1));
            presenter.Bind(campaign);
            campaign.Start();

            campaign.UseSkill(TestCampaigns.Strike);

            Assert.That(view.Lines, Is.EqualTo(new[] { "슬라임은 쓰러졌다." }));
        }

        [Test]
        public void 전투가_아닌_단계의_결과에는_이전_전투의_처치_문구가_나오지_않아야_한다()
        {
            var texts = new FakeEncounterTextProvider();
            texts.Slain["slime"] = "슬라임은 쓰러졌다.";
            var view = new FakeStageResultView();
            var presenter = new StageResultPresenter(view, new ChoicePresenter(new FakeChoiceView()), texts);
            var chapter = new ChapterDefinition(
                "c1", "c1",
                new[] { new StageDefinition(StageType.Battle), new StageDefinition(StageType.Episode) },
                new[] { TestCampaigns.Monster("slime", 30, 1) });
            var campaign = new CampaignManager(
                new[] { chapter }, TestCampaigns.Hero(), new[] { TestCampaigns.Strike },
                new SkillDefinition[0], new SequenceRandomSource(0, 0, 0, 0));
            presenter.Bind(campaign);
            campaign.Start();
            campaign.UseSkill(TestCampaigns.Strike); // 전투 단계 결과 (처치 문구가 나옴)
            campaign.Continue();                      // 결과 -> 에피소드
            campaign.Continue();                      // 에피소드를 마침 -> 에피소드 결과

            Assert.That(view.Lines, Is.Empty);
        }
    }
}