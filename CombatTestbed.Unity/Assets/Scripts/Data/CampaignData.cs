using System;
using System.Collections.Generic;
using System.Linq;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Definitions;
using UnityEngine;

/// <summary>
/// 한 판(캠페인)의 전체 구성을 묶은 최상위 데이터 에셋입니다.
/// 게임 시작 시 GameBootstrapper가 이 에셋 하나로 게임을 조립합니다.
/// </summary>
[CreateAssetMenu(menuName = "Combat Testbed/Campaign", fileName = "Campaign_New")]
public class CampaignData : ScriptableObject
{
    [Header("플레이어")]
    [SerializeField] private ActorData _player;

    [Header("스킬")]
    [Tooltip("처음부터 배우고 있는 스킬 (1개 이상)")]
    [SerializeField] private List<SkillData> _startingSkills = new List<SkillData>();

    [Tooltip("진행 중에 무작위로 배울 수 있는 스킬 풀 (이미 아는 스킬은 자동 제외)")]
    [SerializeField] private List<SkillData> _learnableSkills = new List<SkillData>();

    [Header("챕터 (위에서부터 진행)")]
    [SerializeField] private List<ChapterData> _chapters = new List<ChapterData>();

    public string PlayerId => RequirePlayer().Id;
    public string PlayerName => RequirePlayer().DisplayName;

    /// <summary>
    /// 플레이어 정의를 새로 만들어 돌려줍니다. (스윕에서 값 일부만 바꿔 쓰는 용도)
    /// </summary>
    public ActorDefinition PlayerDefinition => RequirePlayer().ToDefinition();

    /// <summary>
    /// 이 데이터로 새 캠페인을 만듭니다. 호출할 때마다 새 정의 객체를 만들어서, 다시 시작해도 안전합니다.
    /// </summary>
    /// <param name="playerOverride">지정하면 SO의 플레이어 대신 이 정의를 씁니다. (스윕에서 스탯을 바꿔 볼 때 사용)</param>
    public CampaignManager CreateCampaign(IRandomSource random, ActorDefinition playerOverride = null)
    {
        var chapters = DataGuard.ConvertAll(this, nameof(_chapters), _chapters, c => c.ToDefinition());
        var starting = DataGuard.ConvertAll(this, nameof(_startingSkills), _startingSkills, s => s.ToDefinition());
        var learnable = DataGuard.ConvertAll(this, nameof(_learnableSkills), _learnableSkills, s => s.ToDefinition());

        ActorDefinition player = playerOverride ?? RequirePlayer().ToDefinition();

        return new CampaignManager(chapters, player, starting, learnable, random);
    }

    private ActorData RequirePlayer()
    {
        if (_player == null)
        {
            throw new InvalidOperationException($"[{name}] Player가 비어 있습니다.");
        }

        return _player;
    }

#if UNITY_EDITOR
    public void Setup(
        ActorData player, IEnumerable<SkillData> startingSkills,
        IEnumerable<SkillData> learnableSkills, IEnumerable<ChapterData> chapters)
    {
        _player = player;
        _startingSkills = startingSkills.ToList();
        _learnableSkills = learnableSkills.ToList();
        _chapters = chapters.ToList();
    }
#endif
}