using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Presentation.Navigation;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Navigation
{
    [TestFixture]
    public class ScreenNavigatorTests
    {
        private static string Label(ScreenId? id)
        {
            return id.HasValue ? id.Value.ToString() : "None";
        }

        // 화면 변경 통보도 같은 로그에 기록해 Show/Hide와의 순서를 함께 검증
        private static ScreenNavigator CreateNavigator(FakeScreenSet screens)
        {
            var navigator = new ScreenNavigator(screens.AsViews());
            navigator.ScreenChanged += (previous, next) => screens.Log.Add($"Changed:{Label(previous)}->{next}");
            return navigator;
        }

        [Test]
        public void 처음에는_화면이_없고_시작_화면으로_이동하면_그_화면만_보여야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = CreateNavigator(screens);
            Assert.That(navigator.Current, Is.Null);

            navigator.NavigateTo(ScreenId.Title);

            Assert.That(navigator.Current, Is.EqualTo(ScreenId.Title));
            Assert.That(screens.Views[ScreenId.Title].IsVisible, Is.True);
            Assert.That(screens.Views.Values.Count(v => v.IsVisible), Is.EqualTo(1)); // 보이는 화면은 항상 하나
            Assert.That(screens.Log.Entries, Is.EqualTo(new[] { "Changed:None->Title", "Show:Title" }));
        }

        [Test]
        public void 화면을_이동하면_이전_화면을_숨기고_변경을_알린_뒤_새_화면을_보여야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = CreateNavigator(screens);
            navigator.NavigateTo(ScreenId.Title);

            navigator.NavigateTo(ScreenId.Story);
            navigator.NavigateTo(ScreenId.StageResult);

            Assert.That(screens.Log.Entries, Is.EqualTo(new[]
            {
                "Changed:None->Title", "Show:Title",
                "Hide:Title", "Changed:Title->Story", "Show:Story",
                "Hide:Story", "Changed:Story->StageResult", "Show:StageResult"
            }));
            Assert.That(screens.Views.Values.Count(v => v.IsVisible), Is.EqualTo(1));
            Assert.That(navigator.Current, Is.EqualTo(ScreenId.StageResult));
        }

        [Test]
        public void 허용되지_않은_이동은_예외가_발생하고_화면은_그대로여야_한다()
        {
            var screens = new FakeScreenSet();
            var navigator = CreateNavigator(screens);

            Assert.Throws<InvalidOperationException>(() => navigator.NavigateTo(ScreenId.Story)); // 처음에는 Title만 가능

            navigator.NavigateTo(ScreenId.Title);
            int logCountBefore = screens.Log.Entries.Count;

            Assert.Throws<InvalidOperationException>(() => navigator.NavigateTo(ScreenId.Victory));
            Assert.Throws<InvalidOperationException>(() => navigator.NavigateTo(ScreenId.Title)); // 같은 화면으로는 이동 불가

            Assert.That(navigator.Current, Is.EqualTo(ScreenId.Title));
            Assert.That(screens.Log.Entries.Count, Is.EqualTo(logCountBefore)); // Show/Hide/통보 없음
        }

        [Test]
        public void 모든_화면의_View가_없으면_네비게이터를_만들_수_없어야_한다()
        {
            var screens = new FakeScreenSet();

            var missingVictory = screens.AsViews();
            missingVictory.Remove(ScreenId.Victory);

            var nullBattle = screens.AsViews();
            nullBattle[ScreenId.Battle] = null;

            Assert.Throws<ArgumentNullException>(() => new ScreenNavigator(null));
            Assert.Throws<ArgumentException>(() => new ScreenNavigator(missingVictory));
            Assert.Throws<ArgumentException>(() => new ScreenNavigator(nullBattle));
        }
    }
}