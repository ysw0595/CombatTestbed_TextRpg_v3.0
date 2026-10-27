#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Choices;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 단계 결과 화면의 Presenter입니다. 받은 보상과 새로 배운 스킬을 문구로 보여 줍니다.
    /// 구독이 있으므로 사용이 끝나면 Dispose해야 합니다.
    /// </summary>
    public sealed class StageResultPresenter : IChoiceHandler, IDisposable
    {
        private readonly IStageResultView _view;
        private readonly ChoicePresenter _choices;
        private CampaignManager? _campaign;
        private bool _disposed;

        public StageResultPresenter(IStageResultView view, ChoicePresenter choices)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
        }

        public void Bind(CampaignManager campaign)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(StageResultPresenter));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            Unbind();

            _campaign = campaign;
            _campaign.StageCleared += OnStageCleared;
        }

        private void Unbind()
        {
            if (_campaign == null) return;

            _campaign.StageCleared -= OnStageCleared;
            _campaign = null;
        }

        private void OnStageCleared(StageClearResult result)
        {
            var lines = new List<string>();

            // 새로 배운 스킬을 먼저, 그다음 보상 (원본의 스킬 획득 -> 체력 회복 순서)
            if (result.LearnedSkill != null)
            {
                lines.Add(GameMessages.SkillLearned(result.LearnedSkill));
            }

            foreach (EffectOutcome outcome in result.Rewards)
            {
                string? line = RewardMessages.Format(outcome);
                if (line != null)
                {
                    lines.Add(line);
                }
            }

            _view.ShowResult(GameMessages.StageClearedTitle(result.Stage), lines);
            _choices.SetChoices(new[]
            {
                new ActionChoice(GameMessages.ContinueChoiceId, GameMessages.ContinueLabel)
            });
        }

        public void HandleChoice(ActionChoice choice)
        {
            if (_campaign == null || choice.Id != GameMessages.ContinueChoiceId) return;
            if (_campaign.Phase != CampaignPhase.StageResult) return;

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