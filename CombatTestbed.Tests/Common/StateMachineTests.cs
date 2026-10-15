using System;
using NUnit.Framework;
using CombatTestbed.Core.Common;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Common
{
    [TestFixture]
    public class StateMachineTests
    {
        // 진입/이탈을 로그에 기록하고, 필요하면 진입 때 추가 동작(예: 다음 전이 요청)을 하는 테스트용 상태
        private class DelegateState : IState
        {
            private readonly EventLog _log;
            private readonly Action? _onEnter;

            public string Name { get; }

            public DelegateState(string name, EventLog log, Action? onEnter = null)
            {
                Name = name;
                _log = log;
                _onEnter = onEnter;
            }

            public void Enter()
            {
                _log.Add($"Enter:{Name}");
                _onEnter?.Invoke();
            }

            public void Exit()
            {
                _log.Add($"Exit:{Name}");
            }
        }

        private static string Label(DelegateState? state)
        {
            return state == null ? "None" : state.Name;
        }

        // StateChanged도 같은 로그에 기록해 Enter/Exit와의 순서를 함께 검증
        private StateMachine<DelegateState> CreateMachine(EventLog log)
        {
            var machine = new StateMachine<DelegateState>();
            machine.StateChanged += (previous, next) =>
                log.Add($"Changed:{Label(previous)}->{Label(next)}");
            return machine;
        }

        [Test]
        public void 처음_전이하면_Enter만_호출되고_이전_상태가_없다는_통보가_발행되어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var a = new DelegateState("A", log);
            Assert.That(machine.Current, Is.Null);

            machine.ChangeState(a);

            Assert.That(machine.Current, Is.SameAs(a));
            Assert.That(log.Entries, Is.EqualTo(new[] { "Enter:A", "Changed:None->A" }));
        }

        [Test]
        public void 다른_상태로_전이하면_이전_상태_Exit_새_상태_Enter_통보_순서여야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var a = new DelegateState("A", log);
            var b = new DelegateState("B", log);
            machine.ChangeState(a);

            machine.ChangeState(b);

            Assert.That(machine.Current, Is.SameAs(b));
            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Enter:A", "Changed:None->A",
                "Exit:A", "Enter:B", "Changed:A->B"
            }));
        }

        [Test]
        public void 같은_상태로_전이하면_Exit_후_다시_Enter하는_재진입이어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var a = new DelegateState("A", log);
            machine.ChangeState(a);

            machine.ChangeState(a);

            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Enter:A", "Changed:None->A",
                "Exit:A", "Enter:A", "Changed:A->A"
            }));
        }

        [Test]
        public void Enter_안에서_전이를_요청하면_진행중인_전이가_끝난_뒤_순서대로_처리되어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var a = new DelegateState("A", log);
            var c = new DelegateState("C", log);
            var b = new DelegateState("B", log, () => machine.ChangeState(c)); // B 진입 중 C로 전이 요청
            machine.ChangeState(a);

            machine.ChangeState(b);

            // A->B 통보가 먼저, 그 뒤에 B->C가 처리됨 (통보 순서가 뒤집히지 않음)
            Assert.That(machine.Current, Is.SameAs(c));
            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Enter:A", "Changed:None->A",
                "Exit:A", "Enter:B", "Changed:A->B",
                "Exit:B", "Enter:C", "Changed:B->C"
            }));
        }

        [Test]
        public void Enter_안의_전이_요청이_연쇄되어도_모두_순서대로_처리되어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var d = new DelegateState("D", log);
            var c = new DelegateState("C", log, () => machine.ChangeState(d));
            var b = new DelegateState("B", log, () => machine.ChangeState(c));

            machine.ChangeState(b);

            Assert.That(machine.Current, Is.SameAs(d));
            Assert.That(log.Entries, Is.EqualTo(new[]
            {
                "Enter:B", "Changed:None->B",
                "Exit:B", "Enter:C", "Changed:B->C",
                "Exit:C", "Enter:D", "Changed:C->D"
            }));
        }

        [Test]
        public void null_상태로_전이를_요청하면_예외가_발생하고_현재_상태는_유지되어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var a = new DelegateState("A", log);
            machine.ChangeState(a);
            int countBefore = log.Entries.Count;

            Assert.Throws<ArgumentNullException>(() => machine.ChangeState(null));

            Assert.That(machine.Current, Is.SameAs(a));
            Assert.That(log.Entries.Count, Is.EqualTo(countBefore));
        }

        [Test]
        public void Enter에서_예외가_발생해도_대기중인_요청이_정리되고_머신을_다시_사용할_수_있어야_한다()
        {
            var log = new EventLog();
            var machine = CreateMachine(log);
            var c = new DelegateState("C", log);
            var bad = new DelegateState("Bad", log, () =>
            {
                machine.ChangeState(c); // 요청을 남기고
                throw new InvalidOperationException("의도된 실패"); // 실패
            });
            var good = new DelegateState("Good", log);

            Assert.Throws<InvalidOperationException>(() => machine.ChangeState(bad));
            machine.ChangeState(good);

            Assert.That(machine.Current, Is.SameAs(good));
            Assert.That(log.Entries, Does.Contain("Enter:Good"));
            Assert.That(log.Entries, Does.Not.Contain("Enter:C")); // 실패 전에 남긴 요청은 폐기됨
        }
    }
}