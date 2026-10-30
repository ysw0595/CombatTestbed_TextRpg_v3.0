using System;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Content;
using CombatTestbed.Presentation.Screens;

namespace CombatTestbed.Tests.Presentation
{
    [TestFixture]
    public class ScenarioTextProviderTests
    {
        // 요청된 키를 그대로 문구에 담아 돌려주는 가짜 loader: 결과 문구만 보면 어떤 키가 요청됐는지 알 수 있음
        private static readonly Func<string, string?> EchoLoader = key => "내용:" + key;

        private static StageInfo StageAt(int chapterIndex, int stageIndex)
        {
            ChapterDefinition chapter = DefaultChapters.All[chapterIndex];
            return new StageInfo(chapterIndex, stageIndex, chapter, chapter.Stages[stageIndex]);
        }

        [Test]
        public void 프롤로그_단계는_챕터_번호가_들어간_Prologue_키로_문구를_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            string text = provider.GetStageText(StageAt(0, 0));

            Assert.That(text, Is.EqualTo("내용:Chapter1_Prologue"));
        }

        [Test]
        public void 에피소드_단계는_해당_챕터의_Episode_키로_문구를_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            string text = provider.GetStageText(StageAt(1, 5)); // 챕터 2의 에피소드

            Assert.That(text, Is.EqualTo("내용:Chapter2_Episode"));
        }

        [Test]
        public void 스토리가_아닌_단계의_문구를_요청하면_ArgumentException을_던져야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            Assert.Throws<ArgumentException>(() => provider.GetStageText(StageAt(0, 1))); // 전투 단계
        }

        [Test]
        public void 클리어_엔딩은_Victory_키로_문구를_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            string text = provider.GetEndingText(CampaignResult.Victory, StageAt(2, 9));

            Assert.That(text, Is.EqualTo("내용:Victory"));
        }

        [Test]
        public void 일반_몬스터에게_패배하면_Defeat_Mob_키로_문구를_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            string text = provider.GetEndingText(CampaignResult.Defeat, StageAt(0, 2));

            Assert.That(text, Is.EqualTo("내용:Defeat_Mob"));
        }

        [Test]
        public void 보스에게_패배하면_그_보스의_Id가_들어간_키로_문구를_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            string text = provider.GetEndingText(CampaignResult.Defeat, StageAt(1, 9)); // 챕터 2 보스: 반 헬싱

            Assert.That(text, Is.EqualTo("내용:Defeat_vanhelsing"));
        }

        [Test]
        public void 문구가_없으면_빠진_키를_알려_주는_대체_문구를_돌려줘야_한다()
        {
            var provider = new ScenarioTextProvider(key => null);

            string text = provider.GetStageText(StageAt(0, 0));

            Assert.That(text, Does.Contain("Chapter1_Prologue"));
        }

        [Test]
        public void loader가_null이면_ArgumentNullException을_던져야_한다()
        {
            Assert.Throws<ArgumentNullException>(() => new ScenarioTextProvider(null));
        }

        [Test]
        public void 등장_문구는_Appear_액터Id_키로_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            Assert.That(provider.GetAppearText("farmer"), Is.EqualTo("내용:Appear_farmer"));
        }

        [Test]
        public void 처치_문구는_Slain_액터Id_키로_불러와야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            Assert.That(provider.GetSlainText("priest"), Is.EqualTo("내용:Slain_priest"));
        }

        [Test]
        public void 연출_문구가_없으면_대체_문구가_아니라_null을_돌려줘야_한다()
        {
            var provider = new ScenarioTextProvider(key => null);

            Assert.That(provider.GetAppearText("farmer"), Is.Null);
            Assert.That(provider.GetSlainText("farmer"), Is.Null);
        }

        [Test]
        public void 공백만_있는_문구는_없는_것으로_취급해야_한다()
        {
            var provider = new ScenarioTextProvider(key => "  \r\n\n ");

            Assert.That(provider.GetAppearText("farmer"), Is.Null);
        }

        [Test]
        public void 문구의_앞뒤_빈_줄은_제거하고_줄바꿈은_통일해야_한다()
        {
            var provider = new ScenarioTextProvider(key => "\r\n첫 줄\r\n둘째 줄\r\n\r\n");

            Assert.That(provider.GetAppearText("farmer"), Is.EqualTo("첫 줄\n둘째 줄"));
        }

        [Test]
        public void 액터_Id가_비어_있으면_ArgumentException을_던져야_한다()
        {
            var provider = new ScenarioTextProvider(EchoLoader);

            Assert.Throws<ArgumentException>(() => provider.GetAppearText(" "));
        }
    }
}