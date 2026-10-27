using System;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Navigation;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Navigation
{
    [TestFixture]
    public class ScreenFlowPresenterTests
    {
        private static readonly SkillDefinition Strike = new SkillDefinition("strike", "strike", new[]
        {
            SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
        });

        private static ChapterDefinition PrologueChapter()
        {
            return new ChapterDefinition("c", "c", new[] { new StageDefinition(StageType.Prologue) });
        }

        private static CampaignManager CreateCampaign(ChapterDefinition chapter, SequenceRandomSource random)
        {
            var hero = new ActorDefinition("hero", "용사", ActorRole.Player, 100, 50);
            return new CampaignManager(new[] { chapter }, hero, new[] { Strike }, new SkillDefinition[0], random);
        }

        [Test]
        public void 캠페인을_시작하면_시작_화면에서_첫_단계의_화면으로_이동해야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            using (var flow = new ScreenFlowPresenter(navigator))
            {
                var campaign = CreateCampaign(PrologueChapter(), new SequenceRandomSource());
                navigator.NavigateTo(ScreenId.Title);
                flow.Bind(campaign);

                campaign.Start();

                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Story));
            }
        }

        [Test]
        public void 기본_캠페인을_끝까지_진행하면_모든_화면_이동이_규칙에_맞고_클리어_화면으로_끝나야_한다()
        {
            var strongHero = new ActorDefinition("hero", "용사", ActorRole.Player, 1000, 100000);
            var campaign = DefaultCampaign.Create(new SystemRandomSource(7), strongHero);
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            using (var flow = new ScreenFlowPresenter(navigator))
            {
                navigator.NavigateTo(ScreenId.Title);
                flow.Bind(campaign);
                campaign.Start();

                int guard = 0;
                while (campaign.Phase != CampaignPhase.Victory && campaign.Phase != CampaignPhase.Defeat)
                {
                    Assert.That(++guard, Is.LessThan(500), "캠페인이 끝나지 않습니다.");

                    if (campaign.Phase == CampaignPhase.Battle)
                    {
                        campaign.UseSkill(campaign.SkillBook.Skills[0]);
                    }
                    else
                    {
                        campaign.Continue();
                    }
                }

                // 규칙에 어긋난 이동이 있었다면 위에서 예외가 났을 것임
                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Victory));
                Assert.That(screens.Views[ScreenId.Title].ShowCount, Is.EqualTo(1));
                Assert.That(screens.Views[ScreenId.Story].ShowCount, Is.EqualTo(6));        // 챕터마다 프롤로그 + 에피소드
                Assert.That(screens.Views[ScreenId.Battle].ShowCount, Is.EqualTo(24));      // 챕터마다 전투 7 + 보스 1
                Assert.That(screens.Views[ScreenId.StageResult].ShowCount, Is.EqualTo(30)); // 모든 단계마다 결과 화면
                Assert.That(screens.Views[ScreenId.Victory].ShowCount, Is.EqualTo(1));
                Assert.That(screens.Views[ScreenId.Defeat].ShowCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void 플레이어가_사망하면_게임_오버_화면이_되고_시작_화면으로_돌아가_새_캠페인을_시작할_수_있어야_한다()
        {
            var brute = new ActorDefinition("brute", "브루트", ActorRole.Enemy, 1000, 100); // 용사를 한 방에 쓰러뜨림
            var deadlyChapter = new ChapterDefinition(
                "c", "c", new[] { new StageDefinition(StageType.Battle) }, new[] { brute });
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            using (var flow = new ScreenFlowPresenter(navigator))
            {
                var first = CreateCampaign(deadlyChapter, new SequenceRandomSource(0));
                navigator.NavigateTo(ScreenId.Title);
                flow.Bind(first);
                first.Start();
                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Battle));

                first.UseSkill(Strike); // 반격으로 사망
                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Defeat));

                // 다시 시작: 시작 화면으로 돌아가 새 캠페인에 연결
                navigator.NavigateTo(ScreenId.Title);
                var second = CreateCampaign(PrologueChapter(), new SequenceRandomSource());
                flow.Bind(second);
                second.Start();

                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Story));
            }
        }

        [Test]
        public void 연결_대상을_바꾸면_이전_캠페인은_더_이상_화면을_움직이지_않아야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            using (var flow = new ScreenFlowPresenter(navigator))
            {
                var oldCampaign = CreateCampaign(PrologueChapter(), new SequenceRandomSource());
                var newCampaign = CreateCampaign(PrologueChapter(), new SequenceRandomSource());
                navigator.NavigateTo(ScreenId.Title);
                flow.Bind(oldCampaign);
                flow.Bind(newCampaign);

                oldCampaign.Start(); // 연결이 해제된 캠페인
                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Title));

                newCampaign.Start();
                Assert.That(navigator.Current, Is.EqualTo(ScreenId.Story));
            }
        }

        [Test]
        public void Dispose하면_캠페인_구독이_해제되고_중복_호출은_안전하며_이후_연결은_거부되어야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            var flow = new ScreenFlowPresenter(navigator);
            var campaign = CreateCampaign(PrologueChapter(), new SequenceRandomSource());
            navigator.NavigateTo(ScreenId.Title);
            flow.Bind(campaign);

            flow.Dispose();
            flow.Dispose(); // 중복 호출 안전
            campaign.Start();

            Assert.That(navigator.Current, Is.EqualTo(ScreenId.Title)); // 화면이 움직이지 않음
            Assert.Throws<ObjectDisposedException>(() => flow.Bind(campaign));
        }

        [Test]
        public void 잘못된_입력으로는_Presenter를_만들거나_연결할_수_없어야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = new ScreenNavigator(screens.AsViews());
            var flow = new ScreenFlowPresenter(navigator);

            Assert.Throws<ArgumentNullException>(() => new ScreenFlowPresenter(null));
            Assert.Throws<ArgumentNullException>(() => flow.Bind(null));
        }
    }
}