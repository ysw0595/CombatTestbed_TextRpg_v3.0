using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Core.Skills;
using UnityEngine;

/// <summary>
/// 스킬 하나의 데이터 에셋입니다. 인스펙터에서 효과 수치를 고칩니다.
/// </summary>
[CreateAssetMenu(menuName = "Combat Testbed/Skill", fileName = "Skill_New")]
public class SkillData : ScriptableObject
{
    [Tooltip("스킬을 구분하는 고유 Id입니다. (예: vampire_bat_fire)")]
    [SerializeField] private string _id;

    [Tooltip("화면에 보이는 스킬 이름입니다.")]
    [SerializeField] private string _displayName;

    [Header("효과 목록 (위에서부터 차례로 적용)")]
    [Tooltip(EffectHelp.Text)]
    [SerializeField] private List<SkillEffectSpec> _effects = new List<SkillEffectSpec>();

    public string Id => _id;
    public string DisplayName => _displayName;

    public SkillDefinition ToDefinition()
    {
        return new SkillDefinition(_id, _displayName, _effects.Select(e => e.ToData()));
    }

#if UNITY_EDITOR
    // 에디터 메뉴(기본 데이터 생성)에서만 쓰는 초기화 메서드입니다. 빌드에는 포함되지 않습니다.
    public void Setup(string id, string displayName, IEnumerable<SkillEffectSpec> effects)
    {
        _id = id;
        _displayName = displayName;
        _effects = effects.ToList();
    }
#endif
}

/// <summary>
/// 효과 목록 칸에 마우스를 올렸을 때 보여 줄 설명입니다. (스킬과 보상이 같이 씁니다)
/// </summary>
public static class EffectHelp
{
    public const string Text =
        "Kind에 따라 쓰는 칸이 다릅니다.\n" +
        "- Damage: 효과량 = FlatValue + (사용자의 ScalingStat x Coefficient), DamageType 사용 (True는 방어 무시)\n" +
        "- Heal: 효과량 = FlatValue + (사용자의 ScalingStat x Coefficient)\n" +
        "- StatGain: 증가량 = FlatValue + (받는 쪽의 ScalingStat x Coefficient)\n" +
        "- ApplyStatus: StatusType과 Stacks만 사용 (Stacks는 1 이상)";
}