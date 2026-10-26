using System;

namespace CombatTestbed.Presentation.Choices
{
    /// <summary>
    /// 선택 버튼 하나에 표시할 선택지입니다. (예: 스킬 하나, "다음" 버튼)
    /// </summary>
    public readonly struct ActionChoice
    {
        /// <summary>
        /// 선택지를 구분하는 값. 같은 화면 안에서는 중복될 수 없습니다. (예: 스킬 Id, "continue")
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 버튼에 표시할 문구
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// false면 버튼은 보이지만 선택할 수 없습니다.
        /// </summary>
        public bool IsEnabled { get; }

        public ActionChoice(string id, string label, bool isEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("선택지 Id는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("선택지 문구는 비어 있을 수 없습니다.", nameof(label));
            }

            Id = id;
            Label = label;
            IsEnabled = isEnabled;
        }
    }
}