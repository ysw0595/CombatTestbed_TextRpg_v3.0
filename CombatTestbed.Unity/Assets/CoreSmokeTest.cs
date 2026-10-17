using UnityEngine;
using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Pipeline;

/// <summary>
/// Core 어셈블리가 Unity에서 정상 동작하는지 확인하는 임시 스모크 테스트입니다.
/// </summary>
public class CoreSmokeTest : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] private int playerMaxHp = 100;
    [SerializeField] private int playerAttack = 20;
    [SerializeField] private int playerDefense = 5;

    [Header("몬스터 스탯")]
    [SerializeField] private int monsterMaxHp = 60;
    [SerializeField] private int monsterAttack = 12;
    [SerializeField] private int monsterDefense = 0;

    private TurnManager _turnManager;

    private void Start()
    {
        BattleActor player = CreateActor("player", "용사", ActorRole.Player,
            playerMaxHp, playerAttack, playerDefense);
        BattleActor monster = CreateActor("monster", "고블린", ActorRole.Enemy,
            monsterMaxHp, monsterAttack, monsterDefense);

        _turnManager = new TurnManager(player, monster, new ActionPipeline());
        _turnManager.Pipeline.ActionResolved += OnActionResolved;
        _turnManager.BattleEnded += OnBattleEnded;

        // 전투가 끝날 때까지 한 라운드씩 진행 (방어 무시 최소 1 피해가 있어 반드시 종료됨)
        while (!_turnManager.IsBattleOver)
        {
            _turnManager.ExecuteRound(new AttackAction(player, monster));
        }
    }

    private void OnDestroy()
    {
        // 구독 해제: 도메인은 구독하지 않고, 구독한 쪽(여기서는 이 컴포넌트)이 해제 책임을 진다.
        if (_turnManager == null) return;
        _turnManager.Pipeline.ActionResolved -= OnActionResolved;
        _turnManager.BattleEnded -= OnBattleEnded;
    }

    private void OnActionResolved(ActionResult result)
    {
        Debug.Log(result.Message);
    }

    private void OnBattleEnded(BattleActor winner)
    {
        Debug.Log(winner == null ? "무승부" : $"전투 종료: {winner.Name} 승리");
    }

    private static BattleActor CreateActor(
        string id, string actorName, ActorRole role, int maxHp, int attack, int defense)
    {
        var stats = new StatSheet();
        stats.SetStat(StatType.MaxHp, maxHp);
        stats.SetStat(StatType.Attack, attack);
        stats.SetStat(StatType.Defense, defense);
        return new BattleActor(id, actorName, role, stats);
    }
}