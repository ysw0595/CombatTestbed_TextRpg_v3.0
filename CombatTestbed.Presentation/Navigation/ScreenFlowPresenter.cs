#nullable enable
using System;
using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 캠페인의 단계 변화를 화면 이동으로 연결하는 Presenter입니다.
    /// 캠페인은 다시 시작할 때마다 새로 만들어지므로 Bind로 연결 대상을 바꿀 수 있고,
    /// 사용이 끝나면 Dispose해서 구독을 해제해야 합니다.
    /// </summary>
    public sealed class ScreenFlowPresenter : IDisposable
    {
        private readonly ScreenNavigator _navigator;
        private CampaignManager? _campaign;
        private bool _disposed;

        public ScreenFlowPresenter(ScreenNavigator navigator)
        {
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        /// <summary>
        /// 이 캠페인의 단계 변화를 화면 이동과 연결합니다. 이전에 연결된 캠페인이 있으면 연결을 해제합니다.
        /// </summary>
        public void Bind(CampaignManager campaign)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ScreenFlowPresenter));
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));

            Unbind();

            _campaign = campaign;
            _campaign.PhaseChanged += OnPhaseChanged;
        }

        private void Unbind()
        {
            if (_campaign == null) return;

            _campaign.PhaseChanged -= OnPhaseChanged;
            _campaign = null;
        }

        private void OnPhaseChanged(CampaignPhase previous, CampaignPhase next)
        {
            _navigator.NavigateTo(CampaignScreenMap.FromPhase(next));
        }

        /// <summary>
        /// 캠페인 구독을 해제합니다. (Unity에서는 MonoBehaviour의 OnDestroy에서 호출)
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            Unbind();
        }
    }
}