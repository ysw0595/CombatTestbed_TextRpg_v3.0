using System;
using System.Collections.Generic;
#nullable enable

namespace CombatTestbed.Core.Common
{
    /// <summary>
    /// 현재 상태를 보관하고 상태 전이를 관리하는 범용 상태 머신입니다. (상태 패턴의 Context 역할)
    /// 전투 FSM, UI 네비게이션 FSM 등에서 재사용할 수 있도록 상태 타입을 제네릭으로 받습니다.
    /// </summary>
    public class StateMachine<TState> where TState : class, IState
    {
        private readonly Queue<TState> _pendingTransitions = new Queue<TState>();
        private bool _isTransitioning;

        /// <summary>
        /// 현재 상태. 아직 아무 상태로도 전이하지 않았다면 null입니다.
        /// </summary>
        public TState? Current { get; private set; }

        /// <summary>
        /// 상태 전환이 끝난 직후, 새 상태의 Enter가 호출되기 직전에 발행됩니다. (이전 상태, 새 상태)
        /// 처음 진입이면 이전 상태는 null입니다. 이 시점에 Current는 이미 새 상태입니다.
        /// </summary>
        public event Action<TState?, TState>? StateChanged;

        /// <summary>
        /// 새 상태로 전이합니다. (이전 상태 Exit -> StateChanged -> 새 상태 Enter)
        /// 같은 상태로의 전이는 다시 진입(Exit 후 Enter)합니다.
        /// 전이 도중(주로 Enter 안)에 호출하면 즉시 실행하지 않고, 진행 중인 전이가 끝난 뒤 순서대로 처리합니다.
        /// </summary>
        public void ChangeState(TState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            _pendingTransitions.Enqueue(next);

            // 이미 전이를 처리하는 중이라면, 바깥 루프가 이 요청을 이어서 처리함
            if (_isTransitioning) return;

            _isTransitioning = true;
            try
            {
                while (_pendingTransitions.Count > 0)
                {
                    TState target = _pendingTransitions.Dequeue();
                    TState? previous = Current;

                    previous?.Exit();
                    Current = target;
                    StateChanged?.Invoke(previous, target);   // Enter 직전에 통보
                    target.Enter();
                }
            }
            finally
            {
                // 예외가 나더라도 머신이 다시 쓰일 수 있도록 정리
                _pendingTransitions.Clear();
                _isTransitioning = false;
            }
        }
    }
}