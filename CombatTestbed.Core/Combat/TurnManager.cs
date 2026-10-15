using System;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat.States;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 플레이어 선공, 몬스터 후공의 라운드 순환과 전투 종료 조건을 총괄하는 턴 관리자입니다.
    /// 전투 흐름은 상태 패턴(FSM)으로 관리하고, 진행 상황은 이벤트로 외부에 알립니다.
    /// (이벤트 핸들러는 읽기 전용 관찰자여야 합니다.)
    /// </summary>
    public class TurnManager
    {
        private readonly IActionSelector _monsterSelector;
        private readonly StateMachine<IBattleState> _machine = new StateMachine<IBattleState>();
        private readonly IBattleState _playerTurnState;
        private readonly IBattleState _enemyTurnState;
        private readonly IBattleState _battleOverState;

        public BattleActor Player { get; }
        public BattleActor Monster { get; }
        public ActionPipeline Pipeline { get; }

        /// <summary>
        /// 현재 전투 라운드 (1부터 시작)
        /// </summary>
        public int CurrentRound { get; private set; }

        /// <summary>
        /// 현재 전투 단계. Start() 전에는 NotStarted입니다.
        /// </summary>
        public BattlePhase Phase => _machine.Current?.Phase ?? BattlePhase.NotStarted;

        /// <summary>
        /// 둘 중 하나라도 사망하면 전투 종료
        /// </summary>
        public bool IsBattleOver => Player.Health.IsDead || Monster.Health.IsDead;

        /// <summary>
        /// 승리한 액터 (무승부이거나 전투 중이면 null)
        /// </summary>
        public BattleActor? Winner
        {
            get
            {
                if (!IsBattleOver) return null;
                if (Player.Health.IsDead && Monster.Health.IsDead) return null;
                return Player.Health.IsDead ? Monster : Player;
            }
        }

        /// <summary>
        /// 전투 단계가 바뀔 때 발행됩니다. (이전 단계, 새 단계)
        /// 새 단계에서 일어나는 일(턴 실행 등)의 이벤트보다 먼저 발행됩니다.
        /// 입력 버튼을 켜고 끄는 용도로 사용합니다.
        /// </summary>
        public event Action<BattlePhase, BattlePhase>? PhaseChanged;

        /// <summary>
        /// 라운드가 실제로 진행되기 시작할 때(플레이어 행동이 접수된 직후) 발행됩니다. (라운드 번호)
        /// </summary>
        public event Action<int>? RoundStarted;

        /// <summary>
        /// 액터의 턴이 시작될 때 발행됩니다. 상태이상의 턴 시작 처리보다 먼저 발행됩니다.
        /// </summary>
        public event Action<BattleActor>? TurnStarted;

        /// <summary>
        /// 액터의 턴이 끝났을 때 발행됩니다. TurnStarted가 발행된 턴마다 반드시 한 번 발행됩니다.
        /// (턴 도중 전투가 끝나 일부 처리가 생략된 경우에도 발행됩니다.)
        /// </summary>
        public event Action<BattleActor>? TurnEnded;
        /// <summary>
        /// 액터가 행동할 수 없는 상태(마비 등)여서 행동 없이 턴을 넘길 때 발행됩니다.
        /// 턴 시작 상태이상 처리 뒤, 턴 종료 상태이상 처리 앞에 발행되며 TurnStarted/TurnEnded 사이에 놓입니다.
        /// </summary>
        public event Action<BattleActor>? TurnSkipped;

        /// <summary>
        /// 전투가 끝났을 때 한 번 발행됩니다. (승자, 무승부면 null) 마지막 TurnEnded 다음에 발행됩니다.
        /// </summary>
        public event Action<BattleActor?>? BattleEnded;

        public TurnManager(
            BattleActor player,
            BattleActor monster,
            ActionPipeline pipeline,
            IActionSelector? monsterSelector = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Monster = monster ?? throw new ArgumentNullException(nameof(monster));
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

            // 지정하지 않으면 기본 일반 공격 (기존 동작 유지)
            _monsterSelector = monsterSelector ?? new BasicAttackSelector();

            _playerTurnState = new PlayerTurnState(this);
            _enemyTurnState = new EnemyTurnState(this);
            _battleOverState = new BattleOverState(this);

            // 상태 머신의 전환 통보를 공개용 단계(BattlePhase)로 변환해 외부에 전달
            _machine.StateChanged += (previous, next) =>
                PhaseChanged?.Invoke(previous?.Phase ?? BattlePhase.NotStarted, next.Phase);

            CurrentRound = 1;
        }

        /// <summary>
        /// 전투를 시작하고 플레이어 입력을 기다리는 단계로 들어갑니다.
        /// 플레이어가 시작부터 행동 불능이면 입력을 기다리지 않고 라운드가 자동으로 진행될 수 있습니다.
        /// </summary>
        public void Start()
        {
            if (_machine.Current != null)
            {
                throw new InvalidOperationException("이미 시작된 전투입니다.");
            }

            if (IsBattleOver)
            {
                throw new InvalidOperationException("이미 종료된 전투는 시작할 수 없습니다.");
            }

            _machine.ChangeState(_playerTurnState);
        }

        /// <summary>
        /// 플레이어 행동을 제출합니다. 제출된 행동으로 플레이어 턴이 진행되고, 이어서 적 턴이 자동으로 진행됩니다.
        /// 다음 라운드에서 플레이어가 행동 불능이면 그 턴도 자동으로 넘기며,
        /// 플레이어의 입력이 다시 필요해지거나 전투가 끝나는 시점에 반환됩니다.
        /// </summary>
        public void SubmitPlayerAction(ICombatAction playerAction)
        {
            if (playerAction == null) throw new ArgumentNullException(nameof(playerAction));

            if (_machine.Current == null)
            {
                throw new InvalidOperationException("전투가 시작되지 않았습니다. Start()를 먼저 호출하세요.");
            }

            _machine.Current.SubmitPlayerAction(playerAction);
        }

        /// <summary>
        /// 1개 라운드를 진행합니다. (아직 시작 전이면 먼저 Start한 뒤, 행동 제출 -> 적 턴 -> 라운드 증가)
        /// 상태 머신 도입 이전의 호출 방식과 호환하기 위한 편의 메서드입니다.
        /// 시작하자마자 행동 불능(마비 등)으로 라운드가 이미 자동 진행되었다면, 그 라운드가 이번 호출의
        /// 라운드로 간주되어 전달된 행동은 사용되지 않습니다.
        /// </summary>
        public void ExecuteRound(ICombatAction playerAction)
        {
            if (playerAction == null) throw new ArgumentNullException(nameof(playerAction));

            if (IsBattleOver)
            {
                throw new InvalidOperationException("이미 종료된 전투에서는 라운드를 진행할 수 없습니다.");
            }

            if (_machine.Current == null)
            {
                int roundBeforeStart = CurrentRound;
                Start();

                if (CurrentRound != roundBeforeStart || IsBattleOver)
                {
                    return;
                }
            }

            SubmitPlayerAction(playerAction);
        }

        // ---- 아래는 같은 어셈블리의 전투 상태 클래스들이 사용하는 내부 동작입니다. ----

        internal void BeginRound()
        {
            RoundStarted?.Invoke(CurrentRound);
        }

        internal void AdvanceRound()
        {
            CurrentRound++;
        }

        internal void NotifyBattleEnded()
        {
            BattleEnded?.Invoke(Winner);
        }

        internal ICombatAction SelectMonsterAction()
        {
            return _monsterSelector.SelectAction(Monster, Player)
                ?? throw new InvalidOperationException("몬스터 행동 선택기가 null 행동을 반환했습니다.");
        }

        internal void GoToPlayerTurn() => _machine.ChangeState(_playerTurnState);

        internal void GoToEnemyTurn() => _machine.ChangeState(_enemyTurnState);

        internal void GoToBattleOver() => _machine.ChangeState(_battleOverState);

        /// <summary>
        /// 단일 액터의 턴을 지정한 행동으로 진행하고, 턴의 시작과 끝을 알립니다.
        /// </summary>
        internal void RunTurn(BattleActor actor, BattleActor opponent, ICombatAction action)
        {
            ExecuteTurn(actor, opponent, action);
        }

        /// <summary>
        /// 행동 없이 턴을 넘깁니다. (행동 불능 상태) 상태이상 처리와 이벤트 쌍은 정상 턴과 동일하게 유지됩니다.
        /// </summary>
        internal void SkipTurn(BattleActor actor, BattleActor opponent)
        {
            ExecuteTurn(actor, opponent, null);
        }

        /// <param name="action">실행할 행동. null이면 행동 없이 턴을 넘기는 스킵 턴입니다.</param>
        private void ExecuteTurn(BattleActor actor, BattleActor opponent, ICombatAction? action)
        {
            TurnStarted?.Invoke(actor);

            ProcessTurn(actor, opponent, action);

            // 도중에 어떤 early return이 있었더라도 시작된 턴은 반드시 여기서 끝을 알림
            TurnEnded?.Invoke(actor);
        }

        /// <summary>
        /// 단일 액터의 턴 라이프사이클 (시작 상태이상 -> 행동 실행 또는 스킵 -> 종료 상태이상)을 처리합니다.
        /// </summary>
        private void ProcessTurn(BattleActor actor, BattleActor opponent, ICombatAction? action)
        {
            // 턴 시작 처리: 상태 이상의 구체적인 처리는 액터가 담당
            actor.StartTurn(opponent);
            if (actor.Health.IsDead) return;

            if (action != null)
            {
                // 행동 실행
                Pipeline.Execute(action);
                if (IsBattleOver) return;
            }
            else
            {
                // 행동할 수 없어 턴을 넘기는 경우
                TurnSkipped?.Invoke(actor);
            }

            // 턴 종료 처리
            actor.EndTurn(opponent);
        }
    }
}