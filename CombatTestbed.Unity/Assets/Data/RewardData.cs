using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using UnityEngine;

/// <summary>
/// 몬스터 처치나 챕터 진행 보상 하나의 데이터 에셋입니다. 효과 대상은 모두 Self여야 합니다.
/// </summary>
[CreateAssetMenu(menuName = "Combat Testbed/Reward", fileName = "Reward_New")]
public class RewardData : ScriptableObject
{
    [SerializeField] private string _id;

    [Header("효과 목록 (대상은 모두 Self)")]
    [Tooltip(EffectHelp.Text)]
    [SerializeField] private List<SkillEffectSpec> _effects = new List<SkillEffectSpec>();

    public string Id => _id;

    public RewardDefinition ToDefinition()
    {
        return new RewardDefinition(_id, _effects.Select(e => e.ToData()));
    }

#if UNITY_EDITOR
    public void Setup(string id, IEnumerable<SkillEffectSpec> effects)
    {
        _id = id;
        _effects = effects.ToList();
    }
#endif
}