#nullable enable
using System;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Choices;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 종료 화면(클리어/게임 오버)의 Presenter입니다. 구독이 있으므로 사용이 끝나면 Dispose해야 합니다.
    /// </summary>
    public sealed class EndingPresenter : IChoiceHandler, IDisposable
    {
        private readonly IEndingView _view;
        private readonly ChoicePresenter _choices;
        private readonly IStoryTextProvider _texts;
        private CampaignManager? _campaign;
        private StageInfo _lastStage;
        private bool _disposed;

        public EndingPresenter(IEndingView view, ChoicePresenter choices, IStoryTextProvider texts)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            _texts = texts ?? throw new ArgumentNullException(nameof(texts));
        }

        /// <summary>
        /// 다시 시작해 달라는 요청입니다.
        /// </summary>
        public event Action? RestartRequested;

        /// <summary>
        /// 게임을 종료해 달라는 요청입니다. (실제 종료는 앱을 가진 쪽의 몫)
        /// </summary>
        public event Action? QuitRequested;

        public void Bind(CampaignManager campaign)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EndingPresenter));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            Unbind();

            _campaign = campaign;
            _campaign.StageStarted += OnStageStarted;
            _campaign.CampaignEnded += OnCampaignEnded;
        }

        private void Unbind()
        {
            if (_campaign == null) return;

            _campaign.StageStarted -= OnStageStarted;
            _campaign.CampaignEnded -= OnCampaignEnded;
            _campaign = null;
        }

        private void OnStageStarted(StageInfo stage)
        {
            _lastStage = stage; // 마지막으로 진행한 단계를 기억해 두었다가 종료 문구 선택에 사용
        }

        private void OnCampaignEnded(CampaignResult result)
        {
            _view.ShowEnding(GameMessages.EndingTitle(result), _texts.GetEndingText(result, _lastStage));
            _choices.SetChoices(new[]
            {
                new ActionChoice(GameMessages.RestartChoiceId, GameMessages.RestartLabel),
                new ActionChoice(GameMessages.QuitChoiceId, GameMessages.QuitLabel)
            });
        }

        public void HandleChoice(ActionChoice choice)
        {
            if (_campaign == null) return;
            if (_campaign.Phase != CampaignPhase.Victory && _campaign.Phase != CampaignPhase.Defeat) return;

            if (choice.Id == GameMessages.RestartChoiceId)
            {
                RestartRequested?.Invoke();
            }
            else if (choice.Id == GameMessages.QuitChoiceId)
            {
                QuitRequested?.Invoke();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            Unbind();
        }
    }
}