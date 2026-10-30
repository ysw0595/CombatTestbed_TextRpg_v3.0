using System;
using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Definitions;
using UnityEngine;

/// <summary>
/// 챕터의 단계 하나를 인스펙터에서 편집하기 위한 직렬화용 데이터입니다.
/// </summary>
[Serializable]
public class StageEntry
{
    [SerializeField] private StageType _type;

    [Tooltip("이 단계를 마치면 무작위 스킬 하나를 배웁니다. (에피소드/보스 단계만)")]
    [SerializeField] private bool _grantsSkill;

    [Tooltip("이 단계를 마치면 적용되는 보상입니다. (에피소드/보스 단계만, 없으면 비워 둠)")]
    [SerializeField] private RewardData _clearReward;

    public StageEntry() { } // Unity 직렬화용 기본 생성자

    public StageEntry(StageType type, bool grantsSkill, RewardData clearReward)
    {
        _type = type;
        _grantsSkill = grantsSkill;
        _clearReward = clearReward;
    }

    public StageDefinition ToDefinition()
    {
        var reward = _clearReward != null ? _clearReward.ToDefinition() : null;
        return new StageDefinition(_type, _grantsSkill, reward);
    }
}

/// <summary>
/// 챕터 하나의 데이터 에셋입니다: 단계 구성, 일반 몬스터 풀, 보스.
/// </summary>
[CreateAssetMenu(menuName = "Combat Testbed/Chapter", fileName = "Chapter_New")]
public class ChapterData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;

    [Header("단계 구성 (위에서부터 진행)")]
    [SerializeField] private List<StageEntry> _stages = new List<StageEntry>();

    [Header("일반 전투에 순환해서 나오는 몬스터")]
    [SerializeField] private List<ActorData> _monsterPool = new List<ActorData>();

    [Header("보스 단계의 보스 (보스 단계가 없으면 비워 둠)")]
    [SerializeField] private ActorData _boss;

    public ChapterDefinition ToDefinition()
    {
        var stages = _stages.Select(s => s.ToDefinition()).ToList();
        var pool = DataGuard.ConvertAll(this, nameof(_monsterPool), _monsterPool, a => a.ToDefinition());
        var boss = _boss != null ? _boss.ToDefinition() : null;

        return new ChapterDefinition(_id, _displayName, stages, pool, boss);
    }

#if UNITY_EDITOR
    public void Setup(
        string id, string displayName, IEnumerable<StageEntry> stages,
        IEnumerable<ActorData> monsterPool, ActorData boss)
    {
        _id = id;
        _displayName = displayName;
        _stages = stages.ToList();
        _monsterPool = monsterPool.ToList();
        _boss = boss;
    }
#endif
}