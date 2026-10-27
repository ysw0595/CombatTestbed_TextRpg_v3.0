using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Game;
using CombatTestbed.Presentation.Navigation;
using CombatTestbed.Presentation.Screens;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Game
{
    [TestFixture]
    public class GameControllerTests
    {
        // 게임 전체를 가짜 View로 조립한 묶음. 선택 입력은 ChoiceView.Select(슬롯)으로 흉내 낸다. (키보드 1번 = 슬롯 0)
        private sealed class GameSetup
        {
            public FakeScreenSet Screens = new FakeScreenSet();
            public FakeChoiceView ChoiceView = new FakeChoiceView();
            public FakeTitleView TitleView = new FakeTitleView();
            public FakeStoryView StoryView = new FakeStoryView();
            public FakeBattleView BattleView = new FakeBattleView();
            public FakeStageResultView ResultView = new FakeStageResultView();
            public FakeEndingView EndingView = new FakeEndingView();
            public ScreenNavigator Navigator;
            public GameController Controller;
            public int FactoryCalls;
            public string LastCharacterId;
            public int QuitCount;
        }

        private static GameSetup CreateGame(Func<CampaignManager> newCampaign)
        {
            var setup = new GameSetup();
            setup.Navigator = new ScreenNavigator(setup.Screens.AsViews());
            var choices = new ChoicePresenter(setup.ChoiceView);
            var texts = new FakeStoryTextProvider();

            setup.Controller = new GameController(
                setup.Navigator,
                new ScreenFlowPresenter(setup.Navigator),
                choices,
                new TitlePresenter(setup.TitleView, choices, new[] { new ActionChoice("vampire", "뱀파이어") }),
                new StoryPresenter(setup.StoryView, choices, texts),
                new BattlePresenter(setup.BattleView, choices),
                new StageResultPresenter(setup.ResultView, choices),
                new EndingPresenter(setup.EndingView, choices, texts),
                characterId =>
                {
                    setup.FactoryCalls++;
                    setup.LastCharacterId = characterId;
                    return newCampaign();
                });
            setup.Controller.QuitRequested += () => setup.QuitCount++;
            return setup;
        }

        private static GameSetup CreateStrongHeroGame()
        {
            // 모든 적을 한 방에 쓰러뜨리는 플레이어: 밸런스와 무관하게 흐름만 검증
            var strongHero = new ActorDefinition("hero", "용사", ActorRole.Player, 1000, 100000);
            return CreateGame(() => DefaultCampaign.Create(new SystemRandomSource(11), strongHero));
        }

        // 항상 첫 번째 선택지(키보드 1번)만 눌러서 진행하다가, 지정한 화면이 되면 멈춘다
        private static void PressFirstChoiceUntil(GameSetup setup, params ScreenId[] stopAt)
        {
            int guard = 0;
            while (!(setup.Navigator.Current.HasValue && stopAt.Contains(setup.Navigator.Current.Value)))
            {
                Assert.That(++guard, Is.LessThan(500), "진행이 끝나지 않습니다.");
                setup.ChoiceView.Select(0);
            }
        }

        [Test]
        public void 게임을_시작하면_시작_화면과_캐릭터_선택지가_보이고_두번_시작할_수_없어야_한다()
        {
            var setup = CreateStrongHeroGame();

            setup.Controller.Start();

            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Title));
            Assert.That(setup.TitleView.Title, Is.EqualTo("IF THE NIGHT COMES"));
            Assert.That(setup.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "vampire" }));
            Assert.Throws<InvalidOperationException>(() => setup.Controller.Start());
        }

        [Test]
        public void 시작_화면에서_첫_번째_선택을_하면_캠페인이_만들어져_첫_스토리_화면이_보여야_한다()
        {
            var setup = CreateStrongHeroGame();
            setup.Controller.Start();

            setup.ChoiceView.Select(0); // 키보드 1번: 뱀파이어

            Assert.That(setup.FactoryCalls, Is.EqualTo(1));
            Assert.That(setup.LastCharacterId, Is.EqualTo("vampire"));
            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Story));
            Assert.That(setup.StoryView.Title, Is.EqualTo("챕터 1 : 폭풍전야 (stage 1)"));
            Assert.That(setup.StoryView.Body, Is.EqualTo("story:chapter1:0"));
            Assert.That(setup.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "continue" }));
        }

        [Test]
        public void 한_번의_선택은_정확히_한_단계만_진행해야_한다()
        {
            var setup = CreateStrongHeroGame();
            setup.Controller.Start();
            setup.ChoiceView.Select(0); // 시작 -> 프롤로그 스토리

            setup.ChoiceView.Select(0); // 스토리의 "다음"

            // 스토리 -> 단계 결과에서 멈춰야 함 (같은 클릭이 단계 결과 화면까지 처리되면 안 됨)
            Assert.That(setup.Controller.Campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.StageResult));
            Assert.That(setup.ResultView.Title, Is.EqualTo("챕터 1 : 폭풍전야 (stage 1) 클리어"));

            setup.ChoiceView.Select(0); // 단계 결과의 "다음"

            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Battle));
            Assert.That(setup.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "vampire_bat_fire" }));
        }

        [Test]
        public void 항상_첫_번째_선택지만_눌러도_게임_한_판이_클리어_화면까지_끝나야_한다()
        {
            var setup = CreateStrongHeroGame();
            setup.Controller.Start();

            PressFirstChoiceUntil(setup, ScreenId.Victory, ScreenId.Defeat);

            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Victory));
            Assert.That(setup.EndingView.Title, Is.EqualTo("게임 클리어"));
            Assert.That(setup.EndingView.Body, Is.EqualTo("ending:Victory:chapter3"));
            Assert.That(setup.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "restart", "quit" }));
            Assert.That(setup.Controller.Campaign.SkillBook.Skills.Count, Is.EqualTo(5));
        }

        [Test]
        public void 종료_화면에서_다시_시작을_고르면_시작_화면으로_돌아가고_새_캠페인을_시작할_수_있어야_한다()
        {
            var setup = CreateStrongHeroGame();
            setup.Controller.Start();
            PressFirstChoiceUntil(setup, ScreenId.Victory);
            var firstCampaign = setup.Controller.Campaign;

            setup.ChoiceView.Select(0); // 다시 시작

            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Title));
            Assert.That(setup.ChoiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "vampire" }));

            setup.ChoiceView.Select(0); // 캐릭터 다시 선택

            Assert.That(setup.FactoryCalls, Is.EqualTo(2));
            Assert.That(setup.Controller.Campaign, Is.Not.SameAs(firstCampaign));
            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Story));
        }

        [Test]
        public void 패배_화면에서_종료를_고르면_종료_요청이_발행되어야_한다()
        {
            // 체력 1, 공격력 1인 약한 플레이어는 첫 전투에서 사망
            var weakHero = new ActorDefinition("hero", "용사", ActorRole.Player, 1, 1);
            var setup = CreateGame(() => DefaultCampaign.Create(new SystemRandomSource(3), weakHero));
            setup.Controller.Start();

            PressFirstChoiceUntil(setup, ScreenId.Defeat, ScreenId.Victory);

            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Defeat));
            Assert.That(setup.EndingView.Title, Is.EqualTo("게임 오버"));
            Assert.That(setup.EndingView.Body, Is.EqualTo("ending:Defeat:chapter1"));

            setup.ChoiceView.Select(1); // 종료

            Assert.That(setup.QuitCount, Is.EqualTo(1));
            Assert.That(setup.Navigator.Current, Is.EqualTo(ScreenId.Defeat)); // 화면은 그대로
        }

        [Test]
        public void Dispose하면_선택_입력_구독이_해제되고_이후_시작은_거부되며_잘못된_입력은_거부되어야_한다()
        {
            var setup = CreateStrongHeroGame();
            setup.Controller.Start();
            Assert.That(setup.ChoiceView.HasSubscribers, Is.True);

            setup.Controller.Dispose();
            setup.Controller.Dispose(); // 중복 호출 안전

            Assert.That(setup.ChoiceView.HasSubscribers, Is.False);
            setup.ChoiceView.Select(0);
            Assert.That(setup.FactoryCalls, Is.EqualTo(0)); // 입력을 더 이상 받지 않음
            Assert.Throws<ObjectDisposedException>(() => setup.Controller.Start());

            var choices = new ChoicePresenter(new FakeChoiceView());
            Assert.Throws<ArgumentNullException>(() => new GameController(
                null, null, choices, null, null, null, null, null, null));
        }
    }
}