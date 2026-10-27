#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Navigation;
using CombatTestbed.Presentation.Screens;

namespace CombatTestbed.Presentation.Game
{
    /// <summary>
    /// 게임 전체의 화면 로직을 묶는 컨트롤러입니다.
    /// - 선택 버튼의 입력을 현재 화면의 Presenter 하나에게만 전달합니다. (한 번의 클릭 = 한 곳에서만 처리)
    /// - 시작 화면에서 게임 시작이 요청되면 새 캠페인을 만들어 모든 Presenter를 연결하고 시작합니다.
    /// - 종료 화면에서 다시 시작이 요청되면 시작 화면으로 돌아갑니다.
    /// 전달받은 Presenter들을 소유하므로, Dispose하면 모두 함께 정리합니다.
    /// </summary>
    public sealed class GameController : IDisposable
    {
        private readonly ScreenNavigator _navigator;
        private readonly ScreenFlowPresenter _flow;
        private readonly ChoicePresenter _choices;
        private readonly TitlePresenter _title;
        private readonly StoryPresenter _story;
        private readonly BattlePresenter _battle;
        private readonly StageResultPresenter _result;
        private readonly EndingPresenter _ending;
        private readonly Func<string, CampaignManager> _campaignFactory;
        private readonly Dictionary<ScreenId, IChoiceHandler> _handlers;

        private bool _started;
        private bool _disposed;

        /// <param name="campaignFactory">캐릭터 선택지 Id로 새 캠페인을 만드는 함수</param>
        public GameController(
            ScreenNavigator navigator,
            ScreenFlowPresenter flow,
            ChoicePresenter choices,
            TitlePresenter title,
            StoryPresenter story,
            BattlePresenter battle,
            StageResultPresenter result,
            EndingPresenter ending,
            Func<string, CampaignManager> campaignFactory)
        {
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _choices = choices ?? throw new ArgumentNullException(nameof(choices));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _story = story ?? throw new ArgumentNullException(nameof(story));
            _battle = battle ?? throw new ArgumentNullException(nameof(battle));
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _ending = ending ?? throw new ArgumentNullException(nameof(ending));
            _campaignFactory = campaignFactory ?? throw new ArgumentNullException(nameof(campaignFactory));

            // 화면마다 선택을 처리할 Presenter를 하나씩 지정
            _handlers = new Dictionary<ScreenId, IChoiceHandler>
            {
                { ScreenId.Title, _title },
                { ScreenId.Story, _story },
                { ScreenId.Battle, _battle },
                { ScreenId.StageResult, _result },
                { ScreenId.Victory, _ending },
                { ScreenId.Defeat, _ending }
            };

            _choices.Chosen += OnChosen;
            _title.StartRequested += OnStartRequested;
            _ending.RestartRequested += OnRestartRequested;
            _ending.QuitRequested += OnQuitRequested;
        }

        /// <summary>
        /// 지금 진행 중인(또는 마지막) 캠페인. 시작 전에는 null입니다.
        /// </summary>
        public CampaignManager? Campaign { get; private set; }

        /// <summary>
        /// 사용자가 종료를 선택했을 때 발행됩니다. (실제로 앱을 끄는 것은 Unity 쪽의 몫)
        /// </summary>
        public event Action? QuitRequested;

        /// <summary>
        /// 게임 화면을 시작합니다. (시작 화면 표시)
        /// </summary>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GameController));
            if (_started) throw new InvalidOperationException("이미 시작되었습니다.");

            _started = true;
            _navigator.NavigateTo(ScreenId.Title);
            _title.Show();
        }

        private void OnChosen(ActionChoice choice)
        {
            // 클릭이 들어온 '그 순간'의 화면을 먼저 확인해서, 그 화면의 Presenter 하나에게만 전달
            ScreenId? current = _navigator.Current;
            if (!current.HasValue) return;

            _handlers[current.Value].HandleChoice(choice);
        }

        private void OnStartRequested(string characterId)
        {
            CampaignManager campaign = _campaignFactory(characterId)
                ?? throw new InvalidOperationException("캠페인 팩토리가 null을 반환했습니다.");

            Campaign = campaign;

            _flow.Bind(campaign);
            _story.Bind(campaign);
            _battle.Bind(campaign);
            _result.Bind(campaign);
            _ending.Bind(campaign);

            campaign.Start();
        }

        private void OnRestartRequested()
        {
            _navigator.NavigateTo(ScreenId.Title);
            _title.Show();
        }

        private void OnQuitRequested()
        {
            QuitRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            _choices.Chosen -= OnChosen;
            _title.StartRequested -= OnStartRequested;
            _ending.RestartRequested -= OnRestartRequested;
            _ending.QuitRequested -= OnQuitRequested;

            _flow.Dispose();
            _story.Dispose();
            _battle.Dispose();
            _result.Dispose();
            _ending.Dispose();
            _choices.Dispose();
        }
    }
}