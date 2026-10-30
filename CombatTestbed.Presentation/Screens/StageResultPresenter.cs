#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Core.Domain.Actors;

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
        private readonly IEncounterTextProvider? _texts;
        private CampaignManager? _campaign;
        private bool _disposed;

        public StageResultPresenter(IStageResultView view, ChoicePresenter choices, IEncounterTextProvider? texts = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            _texts = texts;
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

            // 처치 연출 문구는 맨 앞에 둡니다. (원본: 처치 문구 -> 스킬 획득 -> 보상)
            string? slainText = FindSlainText(result.Stage);
            if (slainText != null)
            {
                if (lines.Count > 0)
                {
                    lines.Insert(0, string.Empty); // 문구와 보상 사이를 한 줄 띄움
                }

                lines.Insert(0, slainText);
            }

            _view.ShowResult(GameMessages.StageClearedTitle(result.Stage), lines);
            _choices.SetChoices(new[]
            {
                new ActionChoice(GameMessages.ContinueChoiceId, GameMessages.ContinueLabel)
            });
        }

        // 전투 단계(일반/보스)에서만 처치한 적의 연출 문구를 찾습니다.
        private string? FindSlainText(StageInfo stage)
        {
            StageType type = stage.Stage.Type;
            bool isBattleStage = type == StageType.Battle || type == StageType.Boss;
            BattleActor? enemy = _campaign?.CurrentEnemy;

            if (_texts == null || !isBattleStage || enemy == null) return null;

            return _texts.GetSlainText(enemy.Id);
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