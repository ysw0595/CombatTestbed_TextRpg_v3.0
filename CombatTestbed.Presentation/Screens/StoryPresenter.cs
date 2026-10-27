#nullable enable
using System;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Choices;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 스토리 화면(프롤로그/에피소드)의 Presenter입니다. 구독이 있으므로 사용이 끝나면 Dispose해야 합니다.
    /// </summary>
    public sealed class StoryPresenter : IChoiceHandler, IDisposable
    {
        private readonly IStoryView _view;
        private readonly ChoicePresenter _choices;
        private readonly IStoryTextProvider _texts;
        private CampaignManager? _campaign;
        private bool _disposed;

        public StoryPresenter(IStoryView view, ChoicePresenter choices, IStoryTextProvider texts)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            _texts = texts ?? throw new ArgumentNullException(nameof(texts));
        }

        public void Bind(CampaignManager campaign)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StoryPresenter));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            Unbind();

            _campaign = campaign;
            _campaign.StageStarted += OnStageStarted;
        }

        private void Unbind()
        {
            if (_campaign == null) return;

            _campaign.StageStarted -= OnStageStarted;
            _campaign = null;
        }

        private void OnStageStarted(StageInfo stage)
        {
            StageType type = stage.Stage.Type;
            if (type != StageType.Prologue && type != StageType.Episode) return;

            _view.ShowStory(GameMessages.StageTitle(stage), _texts.GetStageText(stage));
            _choices.SetChoices(new[]
            {
                new ActionChoice(GameMessages.ContinueChoiceId, GameMessages.ContinueLabel)
            });
        }

        public void HandleChoice(ActionChoice choice)
        {
            if (_campaign == null || choice.Id != GameMessages.ContinueChoiceId) return;
            if (_campaign.Phase != CampaignPhase.Story) return; // 스토리 단계가 아니면 무시

            _campaign.Continue();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            Unbind();
        }
    }
}