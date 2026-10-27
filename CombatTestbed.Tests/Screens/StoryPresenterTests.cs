using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Screens;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Screens
{
    [TestFixture]
    public class StoryPresenterTests
    {
        private static StoryPresenter Create(out FakeStoryView view, out FakeChoiceView choiceView)
        {
            view = new FakeStoryView();
            choiceView = new FakeChoiceView();
            return new StoryPresenter(view, new ChoicePresenter(choiceView), new FakeStoryTextProvider());
        }

        [Test]
        public void 스토리_단계가_시작되면_제목과_문구와_다음_선택지를_보여야_한다()
        {
            var presenter = Create(out var view, out var choiceView);
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(campaign);

            campaign.Start();

            Assert.That(view.Title, Is.EqualTo("챕터 1 : c1 (stage 1)"));
            Assert.That(view.Body, Is.EqualTo("story:c1:0"));
            Assert.That(choiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "continue" }));
            Assert.That(choiceView.Shown[0].Label, Is.EqualTo("다음"));
        }

        [Test]
        public void 전투_단계에서는_스토리_화면을_갱신하지_않아야_한다()
        {
            var presenter = Create(out var view, out var choiceView);
            var campaign = TestCampaigns.SingleStage(
                new StageDefinition(StageType.Battle), TestCampaigns.Monster("slime", 30, 1));
            presenter.Bind(campaign);

            campaign.Start();

            Assert.That(view.ShowCount, Is.EqualTo(0));
            Assert.That(choiceView.ShowCount, Is.EqualTo(0));
        }

        [Test]
        public void 다음을_선택하면_다음_단계로_진행하고_알_수_없는_선택이나_스토리_단계가_아닐_때는_무시되어야_한다()
        {
            var presenter = Create(out _, out _);
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(campaign);
            campaign.Start();
            var next = new ActionChoice("continue", "다음");

            presenter.HandleChoice(new ActionChoice("other", "x"));
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Story));

            presenter.HandleChoice(next);
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));

            Assert.DoesNotThrow(() => presenter.HandleChoice(next)); // 이미 스토리 단계가 아님
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
        }

        [Test]
        public void 연결_교체와_Dispose와_잘못된_입력을_올바르게_처리해야_한다()
        {
            var presenter = Create(out var view, out _);
            var oldCampaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            var newCampaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(oldCampaign);
            presenter.Bind(newCampaign);

            oldCampaign.Start();
            Assert.That(view.ShowCount, Is.EqualTo(0)); // 연결이 해제된 캠페인

            newCampaign.Start();
            Assert.That(view.ShowCount, Is.EqualTo(1));

            presenter.Dispose();
            presenter.Dispose(); // 중복 호출 안전
            Assert.Throws<ObjectDisposedException>(() => presenter.Bind(newCampaign));
            Assert.Throws<ArgumentNullException>(() => Create(out _, out _).Bind(null));
            Assert.Throws<ArgumentNullException>(() =>
                new StoryPresenter(null, new ChoicePresenter(new FakeChoiceView()), new FakeStoryTextProvider()));
        }
    }
}