using System;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Choices
{
    [TestFixture]
    public class ChoicePresenterTests
    {
        private static ActionChoice Choice(string id, bool isEnabled = true)
        {
            return new ActionChoice(id, $"선택-{id}", isEnabled);
        }

        [Test]
        public void 선택지를_설정하면_View에_슬롯_순서대로_전달되고_현재_목록으로_조회되어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);

            presenter.SetChoices(new[] { Choice("a"), Choice("b"), Choice("c") });

            Assert.That(view.Shown.Select(c => c.Id), Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(presenter.Choices.Select(c => c.Id), Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(view.Shown[0].Label, Is.EqualTo("선택-a"));
        }

        [Test]
        public void 슬롯을_선택하면_해당_선택지가_Chosen으로_전달되어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("a"), Choice("b"), Choice("c") });
            var chosen = new System.Collections.Generic.List<string>();
            presenter.Chosen += choice => chosen.Add(choice.Id);

            view.Select(2); // 3번째 슬롯 (키보드 3번)
            view.Select(0); // 첫 번째 슬롯 (키보드 1번)

            Assert.That(chosen, Is.EqualTo(new[] { "c", "a" }));
        }

        [Test]
        public void 선택할_수_없거나_비어_있거나_범위_밖인_슬롯은_무시되어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("a"), Choice("b", isEnabled: false), Choice("c") });
            int chosenCount = 0;
            presenter.Chosen += choice => chosenCount++;

            view.Select(1);  // 비활성 선택지
            view.Select(3);  // 선택지가 없는 슬롯
            view.Select(4);  // 선택지가 없는 슬롯 (키보드 5번)
            view.Select(-1); // 잘못된 번호

            Assert.That(chosenCount, Is.EqualTo(0));
        }

        [Test]
        public void 잘못된_선택지_목록은_예외를_던지고_화면과_현재_목록을_바꾸지_않아야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("keep") });
            int showCountBefore = view.ShowCount;

            var six = Enumerable.Range(1, 6).Select(i => Choice($"id{i}")).ToArray();

            Assert.Throws<ArgumentException>(() => presenter.SetChoices(six));
            Assert.Throws<ArgumentException>(() => presenter.SetChoices(new[] { Choice("dup"), Choice("dup") }));
            Assert.Throws<ArgumentNullException>(() => presenter.SetChoices(null));

            Assert.That(view.ShowCount, Is.EqualTo(showCountBefore));
            Assert.That(presenter.Choices.Select(c => c.Id), Is.EqualTo(new[] { "keep" }));
        }

        [Test]
        public void 선택지는_정확히_5개까지_허용되어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            var five = Enumerable.Range(1, ChoicePresenter.MaxChoices).Select(i => Choice($"id{i}")).ToArray();

            Assert.DoesNotThrow(() => presenter.SetChoices(five));

            Assert.That(view.Shown.Count, Is.EqualTo(5));
            Assert.Throws<ArgumentNullException>(() => new ChoicePresenter(null));
        }

        [Test]
        public void 선택지_자체는_Id나_문구가_비어_있으면_만들_수_없어야_한다()
        {
            Assert.Throws<ArgumentException>(() => new ActionChoice("", "문구"));
            Assert.Throws<ArgumentException>(() => new ActionChoice("id", " "));
            Assert.That(new ActionChoice("id", "문구").IsEnabled, Is.True); // 기본값은 선택 가능
        }

        [Test]
        public void 선택지를_다시_설정하면_이전_목록이_교체되고_선택은_새_목록_기준이어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("old1"), Choice("old2") });
            var chosen = new System.Collections.Generic.List<string>();
            presenter.Chosen += choice => chosen.Add(choice.Id);

            presenter.SetChoices(new[] { Choice("new1") });
            view.Select(0);
            view.Select(1); // 새 목록에는 없는 슬롯

            Assert.That(chosen, Is.EqualTo(new[] { "new1" }));
            Assert.That(view.Shown.Select(c => c.Id), Is.EqualTo(new[] { "new1" }));
        }

        [Test]
        public void Clear하면_선택지가_모두_사라지고_이후_선택은_무시되어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("a") });
            int chosenCount = 0;
            presenter.Chosen += choice => chosenCount++;

            presenter.Clear();
            view.Select(0);

            Assert.That(view.Shown.Count, Is.EqualTo(0));
            Assert.That(presenter.Choices.Count, Is.EqualTo(0));
            Assert.That(chosenCount, Is.EqualTo(0));
        }

        [Test]
        public void Chosen_핸들러_안에서_선택지를_바꿔도_안전하고_전달된_선택지는_선택한_것이어야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("first") });
            ActionChoice received = default;
            presenter.Chosen += choice =>
            {
                received = choice;
                presenter.SetChoices(new[] { Choice("second") }); // 선택 직후 다음 선택지를 주입하는 상황
            };

            view.Select(0);

            Assert.That(received.Id, Is.EqualTo("first"));
            Assert.That(presenter.Choices[0].Id, Is.EqualTo("second"));
            Assert.That(view.Shown[0].Id, Is.EqualTo("second"));
        }

        [Test]
        public void Dispose하면_View_구독이_해제되고_이후_선택은_무시되며_중복_호출도_안전해야_한다()
        {
            var view = new FakeChoiceView();
            var presenter = new ChoicePresenter(view);
            presenter.SetChoices(new[] { Choice("a") });
            int chosenCount = 0;
            presenter.Chosen += choice => chosenCount++;
            Assert.That(view.HasSubscribers, Is.True);

            presenter.Dispose();
            presenter.Dispose(); // 중복 호출 안전

            Assert.That(view.HasSubscribers, Is.False);
            view.Select(0);
            Assert.That(chosenCount, Is.EqualTo(0));
            Assert.Throws<ObjectDisposedException>(() => presenter.SetChoices(new[] { Choice("a") }));
        }
    }
}