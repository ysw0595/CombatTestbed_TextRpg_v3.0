#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Screens;

namespace CombatTestbed.Presentation.Battle
{
    /// <summary>
    /// 전투 화면의 Presenter입니다. 캠페인이 시작하는 전투마다 Core 이벤트를 구독해 View에 전달하고,
    /// 스킬 선택지를 주입하며, 선택된 스킬을 캠페인에 전달합니다.
    /// 구독이 많으므로 사용이 끝나면 반드시 Dispose해서 해제해야 합니다.
    /// </summary>
    public sealed class BattlePresenter : IDisposable, IChoiceHandler
    {
        private readonly IBattleView _view;
        private readonly ChoicePresenter _choices;
        private readonly IEncounterTextProvider? _texts;
        private readonly List<string> _pendingLines = new List<string>();

        private CampaignManager? _campaign;
        private TurnManager? _battle;
        private StageInfo _stage;
        private ActorObserver? _playerObserver;
        private ActorObserver? _enemyObserver;
        private bool _actionResolvedThisTurn;
        private bool _disposed;

        /// <param name="texts">등장 연출 문구 공급자. 생략하면 연출 문구 없이 진행합니다.</param>
        public BattlePresenter(IBattleView view, ChoicePresenter choices, IEncounterTextProvider? texts = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            _texts = texts;
        }

        /// <summary>
        /// 이 캠페인의 전투를 화면에 연결합니다. 이전에 연결된 캠페인이 있으면 연결을 해제합니다.
        /// </summary>
        public void Bind(CampaignManager campaign)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BattlePresenter));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            Unbind();

            _campaign = campaign;
            campaign.StageStarted += OnStageStarted;
            campaign.BattleStarted += OnBattleStarted;
            campaign.StageCleared += OnStageCleared;
            campaign.CampaignEnded += OnCampaignEnded;
        }

        /// <summary>
        /// 사용자가 고른 스킬 선택지를 처리합니다. (알 수 없는 선택지는 무시)
        /// </summary>
        public void HandleChoice(ActionChoice choice)
        {
            if (_campaign == null) return;

            SkillDefinition? skill = _campaign.SkillBook.Skills.FirstOrDefault(s => s.Id == choice.Id);
            if (skill == null) return;

            _campaign.UseSkill(skill);
        }

        private void Unbind()
        {
            UnbindBattle();

            if (_campaign == null) return;

            _campaign.StageStarted -= OnStageStarted;
            _campaign.BattleStarted -= OnBattleStarted;
            _campaign.StageCleared -= OnStageCleared;
            _campaign.CampaignEnded -= OnCampaignEnded;
            _campaign = null;
        }

        // ---- 캠페인 이벤트 ----

        private void OnStageStarted(StageInfo stage)
        {
            _stage = stage;
        }

        private void OnBattleStarted(TurnManager battle)
        {
            UnbindBattle();

            _battle = battle;
            _actionResolvedThisTurn = false;
            _pendingLines.Clear();

            battle.RoundStarted += OnRoundStarted;
            battle.TurnStarted += OnTurnStarted;
            battle.TurnSkipped += OnTurnSkipped;
            battle.TurnEnded += OnTurnEnded;
            battle.BattleEnded += OnBattleEnded;
            battle.Pipeline.ActionResolved += OnActionResolved;

            _playerObserver = new ActorObserver(this, battle.Player);
            _playerObserver.Attach();
            _enemyObserver = new ActorObserver(this, battle.Monster);
            _enemyObserver.Attach();

            _view.ClearLog();
            AppendAppearText(battle.Monster);
            _view.SetHeader(BattleMessages.Header(_stage, 1));
            RefreshActors();
            _choices.SetChoices(BuildSkillChoices());
        }

        private void OnStageCleared(StageClearResult result)
        {
            UnbindBattle();
        }

        private void OnCampaignEnded(CampaignResult result)
        {
            UnbindBattle();
        }

        private void UnbindBattle()
        {
            _playerObserver?.Detach();
            _enemyObserver?.Detach();
            _playerObserver = null;
            _enemyObserver = null;

            if (_battle != null)
            {
                _battle.RoundStarted -= OnRoundStarted;
                _battle.TurnStarted -= OnTurnStarted;
                _battle.TurnSkipped -= OnTurnSkipped;
                _battle.TurnEnded -= OnTurnEnded;
                _battle.BattleEnded -= OnBattleEnded;
                _battle.Pipeline.ActionResolved -= OnActionResolved;
                _battle = null;
            }

            _pendingLines.Clear();
        }

        // ---- 전투(TurnManager) 이벤트 ----

        private void OnRoundStarted(int round)
        {
            _view.SetHeader(BattleMessages.Header(_stage, round));
        }

        private void OnTurnStarted(BattleActor actor)
        {
            _actionResolvedThisTurn = false;
            _pendingLines.Clear();
        }

        private void OnTurnSkipped(BattleActor actor)
        {
            _actionResolvedThisTurn = true;
            _view.AppendLog(BattleMessages.TurnSkipped(actor));
        }

        private void OnActionResolved(ActionResult result)
        {
            _actionResolvedThisTurn = true;
            _view.AppendLog(result.IsSuccess ? result.Message : BattleMessages.ActionFailed(result));
            FlushPending();
            RefreshActors();
        }

        private void OnTurnEnded(BattleActor actor)
        {
            FlushPending();
        }

        private void OnBattleEnded(BattleActor? winner)
        {
            FlushPending();
            RefreshActors();
        }

        // ---- 액터 이벤트 (ActorObserver가 전달) ----

        private void HandleDamageTaken(BattleActor actor, int amount)
        {
            RefreshActors();

            // 행동 중의 피해는 행동 문구에 포함되므로, 행동이 끝난 뒤의 피해(상태이상 틱)만 기록
            if (_actionResolvedThisTurn)
            {
                _view.AppendLog(BattleMessages.StatusDamage(actor, amount));
            }
        }

        private void HandleHealed(BattleActor actor, int amount)
        {
            RefreshActors();
            LogAfterAction(BattleMessages.Healed(actor, amount));
        }

        private void HandleDied(BattleActor actor)
        {
            RefreshActors();
            LogAfterAction(BattleMessages.Died(actor));
        }

        private void HandleEffectApplied(BattleActor owner, IStatusEffect effect)
        {
            RefreshActors();
            LogAfterAction(BattleMessages.StatusApplied(owner, effect));
        }

        // ---- 공통 ----

        // 등장 연출 문구가 있으면 로그의 첫 줄로 보여 줍니다. (선택 사항이라 없으면 아무것도 하지 않음)
        private void AppendAppearText(BattleActor enemy)
        {
            string? text = _texts?.GetAppearText(enemy.Id);
            if (text != null)
            {
                _view.AppendLog(text);
            }
        }

        /// <summary>
        /// 행동이 끝난 뒤라면 바로 기록하고, 행동 중이라면 행동 문구 뒤에 나오도록 보관해 둡니다.
        /// </summary>
        private void LogAfterAction(string line)
        {
            if (_actionResolvedThisTurn)
            {
                _view.AppendLog(line);
            }
            else
            {
                _pendingLines.Add(line);
            }
        }

        private void FlushPending()
        {
            foreach (string line in _pendingLines)
            {
                _view.AppendLog(line);
            }

            _pendingLines.Clear();
        }

        private void RefreshActors()
        {
            if (_battle == null) return;

            _view.ShowActor(ActorSide.Player, ActorViewModel.From(_battle.Player));
            _view.ShowActor(ActorSide.Enemy, ActorViewModel.From(_battle.Monster));
        }

        private IEnumerable<ActionChoice> BuildSkillChoices()
        {
            return _campaign!.SkillBook.Skills.Select(skill => new ActionChoice(skill.Id, skill.Name));
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            Unbind();
        }

        /// <summary>
        /// 액터 한 명의 이벤트를 구독/해제하는 도우미입니다. (해제 시 같은 핸들러가 필요해서 별도 객체로 보관)
        /// </summary>
        private sealed class ActorObserver
        {
            private readonly BattlePresenter _owner;
            private readonly BattleActor _actor;

            public ActorObserver(BattlePresenter owner, BattleActor actor)
            {
                _owner = owner;
                _actor = actor;
            }

            public void Attach()
            {
                _actor.DamageTaken += OnDamageTaken;
                _actor.Healed += OnHealed;
                _actor.Died += OnDied;
                _actor.StatusEffects.EffectApplied += OnEffectApplied;
                _actor.StatusEffects.EffectStacksChanged += OnEffectChanged;
                _actor.StatusEffects.EffectRemoved += OnEffectChanged;
            }

            public void Detach()
            {
                _actor.DamageTaken -= OnDamageTaken;
                _actor.Healed -= OnHealed;
                _actor.Died -= OnDied;
                _actor.StatusEffects.EffectApplied -= OnEffectApplied;
                _actor.StatusEffects.EffectStacksChanged -= OnEffectChanged;
                _actor.StatusEffects.EffectRemoved -= OnEffectChanged;
            }

            private void OnDamageTaken(BattleActor actor, int amount) { _owner.HandleDamageTaken(actor, amount); }
            private void OnHealed(BattleActor actor, int amount) { _owner.HandleHealed(actor, amount); }
            private void OnDied(BattleActor actor) { _owner.HandleDied(actor); }
            private void OnEffectApplied(IStatusEffect effect) { _owner.HandleEffectApplied(_actor, effect); }
            private void OnEffectChanged(IStatusEffect effect) { _owner.RefreshActors(); }
        }
    }
}