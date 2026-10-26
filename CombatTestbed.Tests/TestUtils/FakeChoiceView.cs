using System;
using System.Collections.Generic;
using CombatTestbed.Presentation.Choices;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// Presenter 테스트용 가짜 View입니다. 받은 선택지를 기록하고, 사용자의 선택을 코드로 흉내 냅니다.
    /// </summary>
    public class FakeChoiceView : IChoiceView
    {
        public event Action<int> SlotSelected;

        /// <summary>
        /// 마지막으로 ShowChoices로 받은 선택지
        /// </summary>
        public IReadOnlyList<ActionChoice> Shown { get; private set; } = new List<ActionChoice>();

        /// <summary>
        /// ShowChoices가 호출된 횟수
        /// </summary>
        public int ShowCount { get; private set; }

        /// <summary>
        /// 구독자가 남아 있는지 (구독 해제 검증용)
        /// </summary>
        public bool HasSubscribers => SlotSelected != null;

        public void ShowChoices(IReadOnlyList<ActionChoice> choices)
        {
            Shown = new List<ActionChoice>(choices);
            ShowCount++;
        }

        /// <summary>
        /// 사용자가 슬롯을 선택한 것처럼 이벤트를 발행합니다. (0부터 시작)
        /// </summary>
        public void Select(int slotIndex)
        {
            SlotSelected?.Invoke(slotIndex);
        }
    }
}