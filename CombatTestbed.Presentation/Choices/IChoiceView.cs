#nullable enable
using System;
using System.Collections.Generic;

namespace CombatTestbed.Presentation.Choices
{
    /// <summary>
    /// 선택 버튼 5개를 화면에 보여 주는 View가 따라야 하는 계약입니다.
    /// Unity에서는 uGUI 버튼 5개를 가진 MonoBehaviour가 구현합니다.
    /// </summary>
    public interface IChoiceView
    {
        /// <summary>
        /// 선택지를 슬롯 순서대로 표시합니다. 선택지가 없는 남은 슬롯은 숨깁니다.
        /// </summary>
        void ShowChoices(IReadOnlyList<ActionChoice> choices);

        /// <summary>
        /// 사용자가 슬롯을 선택했을 때 발행합니다. (0부터 시작하는 슬롯 번호: 1번 키/첫 번째 버튼 = 0)
        /// 버튼 클릭과 키보드 입력을 View가 같은 이벤트로 합칩니다.
        /// </summary>
        event Action<int>? SlotSelected;
    }
}