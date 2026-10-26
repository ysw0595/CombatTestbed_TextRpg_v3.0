#nullable enable
using System;
using System.Collections.Generic;

namespace CombatTestbed.Presentation.Choices
{
    /// <summary>
    /// 선택 버튼 5개를 관리하는 Presenter입니다.
    /// 다른 Presenter가 SetChoices로 선택지를 주입하고, 사용자의 선택은 Chosen 이벤트로 돌려받습니다.
    /// View의 이벤트를 구독하므로, 사용이 끝나면 반드시 Dispose해서 구독을 해제해야 합니다.
    /// </summary>
    public sealed class ChoicePresenter : IDisposable
    {
        /// <summary>
        /// 동시에 보여 줄 수 있는 선택지의 최대 개수 (선택 버튼 수)
        /// </summary>
        public const int MaxChoices = 5;

        private readonly IChoiceView _view;
        private readonly List<ActionChoice> _choices = new List<ActionChoice>();
        private bool _disposed;

        public ChoicePresenter(IChoiceView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            Choices = _choices.AsReadOnly();

            _view.SlotSelected += OnSlotSelected;
        }

        /// <summary>
        /// 지금 표시 중인 선택지 (읽기 전용)
        /// </summary>
        public IReadOnlyList<ActionChoice> Choices { get; }

        /// <summary>
        /// 사용자가 선택 가능한 선택지를 골랐을 때 발행됩니다.
        /// </summary>
        public event Action<ActionChoice>? Chosen;

        /// <summary>
        /// 표시할 선택지를 교체합니다. 최대 5개, Id는 중복될 수 없습니다.
        /// 잘못된 입력이면 예외를 던지고 화면은 바뀌지 않습니다.
        /// </summary>
        public void SetChoices(IEnumerable<ActionChoice> choices)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ChoicePresenter));
            if (choices == null) throw new ArgumentNullException(nameof(choices));

            // 먼저 새 목록을 만들어 검증하고, 문제가 없을 때만 현재 상태를 바꿈
            var next = new List<ActionChoice>(choices);

            if (next.Count > MaxChoices)
            {
                throw new ArgumentException($"선택지는 최대 {MaxChoices}개까지 보여 줄 수 있습니다.", nameof(choices));
            }

            var ids = new HashSet<string>();
            foreach (ActionChoice choice in next)
            {
                if (!ids.Add(choice.Id))
                {
                    throw new ArgumentException($"선택지 Id가 중복되었습니다: {choice.Id}", nameof(choices));
                }
            }

            _choices.Clear();
            _choices.AddRange(next);

            _view.ShowChoices(Choices);
        }

        /// <summary>
        /// 모든 선택지를 지웁니다. (연출 중 입력을 막을 때 등)
        /// </summary>
        public void Clear()
        {
            SetChoices(new ActionChoice[0]);
        }

        private void OnSlotSelected(int slotIndex)
        {
            // 빈 슬롯, 범위 밖 번호, 선택할 수 없는 선택지는 무시
            if (slotIndex < 0 || slotIndex >= _choices.Count) return;

            ActionChoice choice = _choices[slotIndex];
            if (!choice.IsEnabled) return;

            Chosen?.Invoke(choice);
        }

        /// <summary>
        /// View 이벤트 구독을 해제합니다. (Unity에서는 MonoBehaviour의 OnDestroy에서 호출)
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _view.SlotSelected -= OnSlotSelected;
        }
    }
}