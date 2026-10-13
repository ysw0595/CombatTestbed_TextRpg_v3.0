using System;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;

namespace CombatTestbed.Core.Combat
{
    /// <summary>
    /// 플레이어 선공, 몬스터 후공의 라운드 순환과 전투 종료 조건을 총괄하는 턴 관리자입니다.
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

            // 1. 플레이어 턴 실행
            ExecuteTurn(Player, playerAction);
            if (IsBattleOver) return;

            // 2. 몬스터 턴 실행 (1단계 검증용 기본 일반 공격)
            var monsterAction = new AttackAction(Monster, Player);
            ExecuteTurn(Monster, monsterAction);
            if (IsBattleOver) return;

            // 3. 둘 다 살아남아 턴을 마치면 라운드 차수 증가
            CurrentRound++;
        }

        /// <summary>
        /// 단일 액터의 턴 라이프사이클 (시작 상태이상 -> 행동 실행 -> 종료 상태이상)을 처리합니다.
        /// </summary>
        private void ExecuteTurn(BattleActor actor, ICombatAction action)
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