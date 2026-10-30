using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CombatTestbed.Core.Balance;
using CombatTestbed.Core.Definitions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 인스펙터에서 고친 캠페인 데이터를 자동으로 수백 판 돌려 보는 밸런스 시뮬레이터 창입니다.
/// 단일 실행과, 플레이어 스탯을 여러 값으로 바꿔 가며 보는 스윕을 지원합니다.
/// </summary>
public class BalanceSimulatorWindow : EditorWindow
{
    private enum SweepStat { Attack, MaxHp }

    private static readonly ISkillPolicy[] Policies =
    {
        new RandomSkillPolicy(),
        new FirstSkillPolicy(),
        new NewestSkillPolicy(),
        new GreedyDamagePolicy()
    };

    private static readonly string[] PolicyNames = Policies.Select(p => p.Name).ToArray();

    [SerializeField] private CampaignData _campaign;
    [SerializeField] private int _games = 1000;
    [SerializeField] private int _seed = 12345;
    [SerializeField] private int _policyIndex;

    [SerializeField] private SweepStat _sweepStat = SweepStat.Attack;
    [SerializeField] private int _sweepFrom = 10;
    [SerializeField] private int _sweepTo = 60;
    [SerializeField] private int _sweepStep = 5;
    [SerializeField] private int _sweepGames = 300;

    private string _report = "";
    private Vector2 _scroll;

    [MenuItem("Combat Testbed/Balance Simulator")]
    public static void Open()
    {
        GetWindow<BalanceSimulatorWindow>("Balance Simulator");
    }

    private void OnEnable()
    {
        if (_campaign == null)
        {
            _campaign = AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/Data/DefaultCampaign.asset");
        }
    }

    private void OnGUI()
    {
        _campaign = (CampaignData)EditorGUILayout.ObjectField("Campaign Data", _campaign, typeof(CampaignData), false);
        _seed = EditorGUILayout.IntField("시드", _seed);
        _policyIndex = EditorGUILayout.Popup("스킬 사용 전략", _policyIndex, PolicyNames);

        using (new EditorGUI.DisabledScope(_campaign == null))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("단일 실행", EditorStyles.boldLabel);
            _games = Mathf.Max(1, EditorGUILayout.IntField("판 수", _games));

            if (GUILayout.Button("시뮬레이션 실행", GUILayout.Height(26)))
            {
                RunSingle();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("파라미터 스윕 (플레이어 스탯을 바꿔 가며 비교)", EditorStyles.boldLabel);
            _sweepStat = (SweepStat)EditorGUILayout.EnumPopup("바꿀 스탯", _sweepStat);
            _sweepFrom = EditorGUILayout.IntField("시작 값", _sweepFrom);
            _sweepTo = EditorGUILayout.IntField("끝 값", _sweepTo);
            _sweepStep = Mathf.Max(1, EditorGUILayout.IntField("간격", _sweepStep));
            _sweepGames = Mathf.Max(1, EditorGUILayout.IntField("값마다 판 수", _sweepGames));

            if (GUILayout.Button("스윕 실행", GUILayout.Height(26)))
            {
                RunSweep();
            }
        }

        if (_report.Length == 0) return;

        EditorGUILayout.Space();

        if (GUILayout.Button("결과 복사"))
        {
            GUIUtility.systemCopyBuffer = _report;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_report);
        EditorGUILayout.EndScrollView();
    }

    private void RunSingle()
    {
        Execute(() =>
        {
            var simulator = new CampaignSimulator(random => _campaign.CreateCampaign(random), Policies[_policyIndex]);
            return simulator.Run(_games, _seed).ToText();
        });
    }

    private void RunSweep()
    {
        Execute(() =>
        {
            if (_sweepTo < _sweepFrom)
            {
                throw new InvalidOperationException("끝 값은 시작 값보다 크거나 같아야 합니다.");
            }

            ActorDefinition basePlayer = _campaign.PlayerDefinition;
            var values = new List<int>();

            for (int value = _sweepFrom; value <= _sweepTo; value += _sweepStep)
            {
                values.Add(value);
            }

            if (values.Count > 200)
            {
                throw new InvalidOperationException($"값이 너무 많습니다({values.Count}개). 간격을 늘려 주세요. (최대 200개)");
            }

            string statName = _sweepStat == SweepStat.Attack ? "플레이어 공격력" : "플레이어 최대 체력";

            SweepResult result = ParameterSweep.Run(
                statName,
                values,
                value => random => _campaign.CreateCampaign(random, WithStat(basePlayer, _sweepStat, value)),
                Policies[_policyIndex],
                _sweepGames,
                _seed);

            return result.ToText();
        });
    }

    // 플레이어 정의에서 스탯 하나만 바꾼 새 정의를 만듭니다. (정의는 불변이라 복사해서 만듦)
    private static ActorDefinition WithStat(ActorDefinition player, SweepStat stat, int value)
    {
        int maxHp = stat == SweepStat.MaxHp ? value : player.MaxHp;
        int attack = stat == SweepStat.Attack ? value : player.Attack;

        return new ActorDefinition(
            player.Id, player.Name, player.Role, maxHp, attack, player.Defense, player.Reward, player.BehaviorSkills);
    }

    // 실행 시간을 재고, 데이터 입력 실수 같은 예외는 창에 이유를 보여 줍니다.
    private void Execute(Func<string> run)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            string text = run();
            stopwatch.Stop();

            _report = text + $"\n(소요 시간: {stopwatch.ElapsedMilliseconds} ms)";
        }
        catch (Exception exception)
        {
            _report = $"실행 실패: {exception.Message}";
            UnityEngine.Debug.LogException(exception);
        }
    }
}