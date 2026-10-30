using System.Collections.Generic;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Rewards;
using UnityEngine;

/// <summary>
/// 플레이어, 몬스터, 보스 하나의 데이터 에셋입니다. 기본 스탯과 보상, 보스 패턴을 인스펙터에서 고칩니다.
/// </summary>
[CreateAssetMenu(menuName = "Combat Testbed/Actor", fileName = "Actor_New")]
public class ActorData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("고유 Id입니다. 시나리오 문구 파일(Appear_{id}, Slain_{id})과 연결되므로 바꾸면 파일 이름도 같이 바꿔야 합니다.")]
    [SerializeField] private string _id;

    [SerializeField] private string _displayName;
    [SerializeField] private ActorRole _role = ActorRole.Enemy;

    [Header("스탯")]
    [SerializeField, Min(1)] private int _maxHp = 1;
    [SerializeField, Min(0)] private int _attack = 1;
    [SerializeField, Min(0)] private int _defense = 0;

    [Header("처치 보상 (없으면 비워 둠)")]
    [SerializeField] private RewardData _reward;

    [Header("행동 스킬 (보스 패턴: 위에서부터 순환, 비어 있으면 일반 공격만)")]
    [SerializeField] private List<SkillData> _behaviorSkills = new List<SkillData>();

    public string Id => _id;
    public string DisplayName => _displayName;

    public ActorDefinition ToDefinition()
    {
        // Unity 오브젝트에는 ?. 를 쓰지 않습니다. 삭제된 참조를 null로 보지 못하기 때문입니다.
        RewardDefinition reward = _reward != null ? _reward.ToDefinition() : null;

        var skills = DataGuard.ConvertAll(this, nameof(_behaviorSkills), _behaviorSkills, s => s.ToDefinition());

        return new ActorDefinition(_id, _displayName, _role, _maxHp, _attack, _defense, reward, skills);
    }

#if UNITY_EDITOR
    public void Setup(
        string id, string displayName, ActorRole role, int maxHp, int attack, int defense,
        RewardData reward, IEnumerable<SkillData> behaviorSkills)
    {
        _id = id;
        _displayName = displayName;
        _role = role;
        _maxHp = maxHp;
        _attack = attack;
        _defense = defense;
        _reward = reward;
        _behaviorSkills = new List<SkillData>(behaviorSkills);
    }
#endif
}