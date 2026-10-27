using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Screens;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Screens
{
    [TestFixture]
    public class TitlePresenterTests
    {
        private static readonly ActionChoice Vampire = new ActionChoice("vampire", "뱀파이어");

        [Test]
        public void 시작_화면을_표시하면_제목과_캐릭터_선택지가_보여야_한다()
        {
            var view = new FakeTitleView();
            var choiceView = new FakeChoiceView();
            var presenter = new TitlePresenter(view, new ChoicePresenter(choiceView), new[] { Vampire });

            presenter.Show();

            Assert.That(view.Title, Is.EqualTo("IF THE NIGHT COMES"));
            Assert.That(view.Subtitle, Is.EqualTo("캐릭터를 선택하세요."));
            Assert.That(choiceView.Shown.Select(c => c.Id), Is.EqualTo(new[] { "vampire" }));
            Assert.That(choiceView.Shown[0].Label, Is.EqualTo("뱀파이어"));
        }

        [Test]
        public void 캐릭터를_선택하면_게임_시작이_요청되고_알_수_없는_선택은_무시되어야_한다()
        {
            var presenter = new TitlePresenter(
                new FakeTitleView(), new ChoicePresenter(new FakeChoiceView()), new[] { Vampire });
            var requested = new List<string>();
            presenter.StartRequested += id => requested.Add(id);

            presenter.HandleChoice(new ActionChoice("unknown", "?"));
            presenter.HandleChoice(Vampire);

            Assert.That(requested, Is.EqualTo(new[] { "vampire" }));
        }

        [Test]
        public void 잘못된_입력으로는_시작_화면_Presenter를_만들_수_없어야_한다()
        {
            var view = new FakeTitleView();
            var choices = new ChoicePresenter(new FakeChoiceView());
            var six = Enumerable.Range(1, 6).Select(i => new ActionChoice($"c{i}", $"캐릭터{i}")).ToArray();

            Assert.Throws<ArgumentNullException>(() => new TitlePresenter(null, choices, new[] { Vampire }));
            Assert.Throws<ArgumentNullException>(() => new TitlePresenter(view, null, new[] { Vampire }));
            Assert.Throws<ArgumentNullException>(() => new TitlePresenter(view, choices, null));
            Assert.Throws<ArgumentException>(() => new TitlePresenter(view, choices, new ActionChoice[0]));
            Assert.Throws<ArgumentException>(() => new TitlePresenter(view, choices, six));
        }
    }
}