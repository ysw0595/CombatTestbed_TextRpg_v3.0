#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 현재 보여 주는 화면을 관리하는 UI 네비게이션 FSM입니다.
    /// 허용되지 않은 화면 이동은 예외로 막습니다.
    /// </summary>
    public sealed class ScreenNavigator
    {
        private readonly Dictionary<ScreenId, ScreenState> _states = new Dictionary<ScreenId, ScreenState>();
        private readonly StateMachine<ScreenState> _machine = new StateMachine<ScreenState>();

        /// <param name="views">모든 화면의 View. 빠진 화면이 있으면 예외입니다.</param>
        public ScreenNavigator(IReadOnlyDictionary<ScreenId, IScreenView> views)
        {
            if (views == null) throw new ArgumentNullException(nameof(views));

            foreach (ScreenId id in (ScreenId[])Enum.GetValues(typeof(ScreenId)))
            {
                if (!views.TryGetValue(id, out IScreenView? view) || view == null)
                {
                    throw new ArgumentException($"{id} 화면의 View가 없습니다.", nameof(views));
                }

                _states[id] = new ScreenState(id, view);
            }

            _machine.StateChanged += (previous, next) => ScreenChanged?.Invoke(previous?.Id, next.Id);
        }

        /// <summary>
        /// 현재 화면. 아직 아무 화면도 보여 주지 않았다면 null입니다.
        /// </summary>
        public ScreenId? Current => _machine.Current?.Id;

        /// <summary>
        /// 화면이 바뀔 때 발행됩니다. (이전 화면, 새 화면) 이전 화면이 숨겨진 뒤, 새 화면이 보이기 직전에 발행됩니다.
        /// </summary>
        public event Action<ScreenId?, ScreenId>? ScreenChanged;

        /// <summary>
        /// 화면을 이동합니다. 허용되지 않은 이동이면 InvalidOperationException을 던지고 화면은 그대로입니다.
        /// </summary>
        public void NavigateTo(ScreenId screen)
        {
            if (!NavigationRules.CanNavigate(Current, screen))
            {
                string from = Current.HasValue ? Current.Value.ToString() : "(없음)";
                throw new InvalidOperationException($"{from} 화면에서 {screen} 화면으로 이동할 수 없습니다.");
            }

            _machine.ChangeState(_states[screen]);
        }
    }
}