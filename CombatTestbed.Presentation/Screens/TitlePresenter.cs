#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Presentation.Choices;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 시작 화면의 Presenter입니다. 캐릭터 선택지를 보여 주고, 선택되면 게임 시작을 요청합니다.
    /// </summary>
    public sealed class TitlePresenter : IChoiceHandler
    {
        private readonly ITitleView _view;
        private readonly ChoicePresenter _choices;
        private readonly List<ActionChoice> _startChoices;

        public TitlePresenter(ITitleView view, ChoicePresenter choices, IEnumerable<ActionChoice> startChoices)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            if (startChoices == null) throw new ArgumentNullException(nameof(startChoices));

            _startChoices = new List<ActionChoice>(startChoices);

            if (_startChoices.Count == 0 || _startChoices.Count > ChoicePresenter.MaxChoices)
            {
                throw new ArgumentException(
                    $"캐릭터 선택지는 1~{ChoicePresenter.MaxChoices}개여야 합니다.", nameof(startChoices));
            }
        }

        /// <summary>
        /// 게임을 시작해 달라는 요청입니다. (선택된 캐릭터의 선택지 Id)
        /// </summary>
        public event Action<string>? StartRequested;

        /// <summary>
        /// 시작 화면의 내용과 선택지를 표시합니다.
        /// </summary>
        public void Show()
        {
            _view.ShowTitle(GameMessages.GameTitle, GameMessages.TitleSubtitle);
            _choices.SetChoices(_startChoices);
        }

        public void HandleChoice(ActionChoice choice)
        {
            if (!_startChoices.Exists(c => c.Id == choice.Id)) return;

            StartRequested?.Invoke(choice.Id);
        }
    }
}