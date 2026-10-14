using System;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 플레이어 선공, 몬스터 후공의 라운드 순환과 전투 종료 조건을 총괄하는 턴 관리자입니다.
    /// 진행 상황은 이벤트로 외부에 알립니다. (핸들러는 읽기 전용 관찰자여야 합니다.)
    /// </summary>
    public class TurnManager
    {
        public BattleActor Player { get; }
        public BattleActor Monster { get; }
        public ActionPipeline Pipeline { get; }

        /// <summary>
        /// 현재 전투 라운드 (1부터 시작)
        /// </summary>
        public int CurrentRound { get; private set; }

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
        /// 라운드가 시작될 때 발행됩니다. (라운드 번호)
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
        /// 전투가 끝났을 때 한 번 발행됩니다. (승자, 무승부면 null) 마지막 TurnEnded 다음에 발행됩니다.
        /// </summary>
        public event Action<BattleActor?>? BattleEnded;

        public TurnManager(BattleActor player, BattleActor monster, ActionPipeline pipeline)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Monster = monster ?? throw new ArgumentNullException(nameof(monster));
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

            CurrentRound = 1;
        }

        /// <summary>
        /// 1개 라운드를 진행합니다. (플레이어 행동 -> 몬스터 행동 -> 라운드 증가)
        /// </summary>
        public void ExecuteRound(ICombatAction playerAction)
        {
            ArgumentNullException.ThrowIfNull(playerAction);

            if (IsBattleOver)
            {
                throw new InvalidOperationException("이미 종료된 전투에서는 라운드를 진행할 수 없습니다.");
            }

            RoundStarted?.Invoke(CurrentRound);

            // 1. 플레이어 턴 실행
            ExecuteTurn(Player, playerAction);

            // 2. 플레이어 턴에서 전투가 끝나지 않았다면 몬스터 턴 실행 (1단계 검증용 기본 일반 공격)
            if (!IsBattleOver)
            {
                var monsterAction = new AttackAction(Monster, Player);
                ExecuteTurn(Monster, monsterAction);
            }

            // 3. 전투가 끝났으면 종료 통보, 둘 다 살아남아 턴을 마쳤으면 라운드 차수 증가
            if (IsBattleOver)
            {
                BattleEnded?.Invoke(Winner);
            }
            else
            {
                CurrentRound++;
            }
        }

        /// <summary>
        /// 단일 액터의 턴을 진행하고, 턴의 시작과 끝을 알립니다.
        /// </summary>
        private void ExecuteTurn(BattleActor actor, ICombatAction action)
        {
            TurnStarted?.Invoke(actor);

            ProcessTurn(actor, action);

            // 도중에 어떤 early return이 있었더라도 시작된 턴은 반드시 여기서 끝을 알림
            TurnEnded?.Invoke(actor);
        }

        /// <summary>
        /// 단일 액터의 턴 라이프사이클 (시작 상태이상 -> 행동 실행 -> 종료 상태이상)을 처리합니다.
        /// </summary>
        private void ProcessTurn(BattleActor actor, ICombatAction action)
        {
            // 턴 시작 처리: 상태 이상의 구체적인 처리는 액터가 담당
            actor.StartTurn();
            if (actor.Health.IsDead) return;

            // 행동 실행
            Pipeline.Execute(action);
            if (IsBattleOver) return;

            // 턴 종료 처리
            actor.EndTurn();
        }
    }
}