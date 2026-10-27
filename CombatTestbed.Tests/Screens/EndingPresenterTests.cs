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
    public class EndingPresenterTests
    {
        private static EndingPresenter Create(out FakeEndingView view, out FakeChoiceView choiceView)
        {
            view = new FakeEndingView();
            choiceView = new FakeChoiceView();
            return new EndingPresenter(view, new ChoicePresenter(choiceView), new FakeStoryTextProvider());
        }

        [Test]
        public void 플레이어가_사망하면_게임_오버_문구와_마지막_단계_정보와_다시_시작_종료_선택지가_나와야_한다()
        {
            var presenter = Create(out var view, out var choiceView);
            var brute = TestCampaigns.Monster("brute", 1000, 100); // 용사를 한 방에 쓰러뜨림
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Battle), brute);
            presenter.Bind(campaign);
            campaign.Start();

            campaign.UseSkill(TestCampaigns.Strike);

            Assert.That(view.Title, Is.EqualTo("게임 오버"));
            Assert.That(view.Body, Is.EqualTo("ending:Defeat:c1"));
            Assert.That(choiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "restart", "quit" }));
            Assert.That(choiceView.Shown.Select(c => c.Label), Is.EqualTo(new[] { "다시 시작", "종료" }));
        }

        [Test]
        public void 모든_단계를_마치면_게임_클리어_문구가_나와야_한다()
        {
            var presenter = Create(out var view, out _);
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(campaign);
            campaign.Start();
            campaign.Continue(); // 프롤로그 -> 단계 결과
            campaign.Continue(); // 단계 결과 -> 승리

            Assert.That(view.Title, Is.EqualTo("게임 클리어"));
            Assert.That(view.Body, Is.EqualTo("ending:Victory:c1"));
        }

        [Test]
        public void 다시_시작과_종료_선택은_각각의_요청으로_전달되고_종료_전이나_알_수_없는_선택은_무시되어야_한다()
        {
            var presenter = Create(out _, out _);
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(campaign);
            int restartCount = 0;
            int quitCount = 0;
            presenter.RestartRequested += () => restartCount++;
            presenter.QuitRequested += () => quitCount++;
            var restart = new ActionChoice("restart", "다시 시작");
            var quit = new ActionChoice("quit", "종료");

            campaign.Start();
            presenter.HandleChoice(restart); // 아직 끝나지 않은 캠페인
            Assert.That(restartCount, Is.EqualTo(0));

            campaign.Continue();
            campaign.Continue(); // 승리
            presenter.HandleChoice(new ActionChoice("other", "x"));
            presenter.HandleChoice(restart);
            presenter.HandleChoice(quit);

            Assert.That(restartCount, Is.EqualTo(1));
            Assert.That(quitCount, Is.EqualTo(1));
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
            oldCampaign.Continue();
            oldCampaign.Continue();
            Assert.That(view.ShowCount, Is.EqualTo(0)); // 연결이 해제된 캠페인

            presenter.Dispose();
            presenter.Dispose();
            Assert.Throws<ObjectDisposedException>(() => presenter.Bind(newCampaign));
            Assert.Throws<ArgumentNullException>(() => Create(out _, out _).Bind(null));
            Assert.Throws<ArgumentNullException>(() =>
                new EndingPresenter(null, new ChoicePresenter(new FakeChoiceView()), new FakeStoryTextProvider()));
        }
    }
}