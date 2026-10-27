using System;
using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Presentation.Navigation;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 화면 패널의 가짜 View입니다. 보이기/숨기기 호출을 횟수와 순서로 기록합니다.
    /// </summary>
    public class FakeScreenView : IScreenView
    {
        private readonly EventLog _log;

        public ScreenId Id { get; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public bool IsVisible { get; private set; }

        public FakeScreenView(ScreenId id, EventLog log)
        {
            Id = id;
            _log = log;
        }

        public void Show()
        {
            ShowCount++;
            IsVisible = true;
            _log.Add($"Show:{Id}");
        }

        public void Hide()
        {
            HideCount++;
            IsVisible = false;
            _log.Add($"Hide:{Id}");
        }
    }

    /// <summary>
    /// 모든 화면의 가짜 View 묶음입니다. 호출 순서는 하나의 로그에 함께 기록됩니다.
    /// </summary>
    public class FakeScreenSet
    {
        public EventLog Log { get; } = new EventLog();
        public Dictionary<ScreenId, FakeScreenView> Views { get; } = new Dictionary<ScreenId, FakeScreenView>();

        public FakeScreenSet()
        {
            foreach (ScreenId id in (ScreenId[])Enum.GetValues(typeof(ScreenId)))
            {
                Views[id] = new FakeScreenView(id, Log);
            }
        }

        public Dictionary<ScreenId, IScreenView> AsViews()
        {
            return Views.ToDictionary(pair => pair.Key, pair => (IScreenView)pair.Value);
        }
    }
}