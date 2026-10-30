using System.Collections.Generic;
using System.IO;
using System.Linq;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 코드로 작성된 기본 콘텐츠(DefaultActors 등)를 SO 에셋으로 한 번에 만들어 주는 에디터 메뉴입니다.
/// 같은 Id의 정의는 에셋 하나를 공유합니다. (예: 늑대는 챕터 1과 2가 같은 에셋을 씁니다)
/// </summary>
public static class DefaultDataGenerator
{
    private const string Root = "Assets/Data";
    private const string CampaignPath = Root + "/DefaultCampaign.asset";

    [MenuItem("Combat Testbed/Generate Default Data")]
    public static void Generate()
    {
        bool exists = AssetDatabase.LoadAssetAtPath<CampaignData>(CampaignPath) != null;

        if (exists && !EditorUtility.DisplayDialog(
                "기본 데이터 생성",
                "이미 기본 데이터가 있습니다.\n다시 생성하면 인스펙터에서 조정한 값이 기본값으로 돌아갑니다. 계속할까요?",
                "덮어쓰기", "취소"))
        {
            return;
        }

        CampaignData campaign = new Builder().Build();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = campaign;
        EditorGUIUtility.PingObject(campaign);

        Debug.Log($"[DefaultDataGenerator] 기본 데이터 생성 완료: {CampaignPath}");
    }

    // 생성 중에 만든 에셋을 Id로 기억해 두는 작업 객체입니다.
    private sealed class Builder
    {
        private readonly Dictionary<string, SkillData> _skills = new Dictionary<string, SkillData>();
        private readonly Dictionary<string, RewardData> _rewards = new Dictionary<string, RewardData>();
        private readonly Dictionary<string, ActorData> _actors = new Dictionary<string, ActorData>();

        public CampaignData Build()
        {
            ActorData player = GetActor(DefaultActors.Vampire);
            List<ChapterData> chapters = DefaultChapters.All.Select(CreateChapter).ToList();
            List<SkillData> starting = new List<SkillData> { GetSkill(DefaultSkills.BatFire) };
            List<SkillData> learnable = DefaultSkills.VampireSkills.Select(GetSkill).ToList();

            CampaignData campaign = LoadOrCreate<CampaignData>(Root, "DefaultCampaign");
            campaign.Setup(player, starting, learnable, chapters);
            EditorUtility.SetDirty(campaign);
            return campaign;
        }

        private SkillData GetSkill(SkillDefinition definition)
        {
            if (_skills.TryGetValue(definition.Id, out SkillData cached)) return cached;

            SkillData asset = LoadOrCreate<SkillData>(Root + "/Skills", definition.Id);
            asset.Setup(definition.Id, definition.Name, definition.Effects.Select(SkillEffectSpec.From));
            EditorUtility.SetDirty(asset);

            _skills[definition.Id] = asset;
            return asset;
        }

        private RewardData GetReward(RewardDefinition definition)
        {
            if (_rewards.TryGetValue(definition.Id, out RewardData cached)) return cached;

            RewardData asset = LoadOrCreate<RewardData>(Root + "/Rewards", definition.Id);
            asset.Setup(definition.Id, definition.Effects.Select(SkillEffectSpec.From));
            EditorUtility.SetDirty(asset);

            _rewards[definition.Id] = asset;
            return asset;
        }

        private ActorData GetActor(ActorDefinition definition)
        {
            if (_actors.TryGetValue(definition.Id, out ActorData cached)) return cached;

            // 참조하는 보상과 스킬 에셋을 먼저 만든 뒤 연결합니다.
            RewardData reward = definition.Reward != null ? GetReward(definition.Reward) : null;
            List<SkillData> skills = definition.BehaviorSkills.Select(GetSkill).ToList();

            ActorData asset = LoadOrCreate<ActorData>(Root + "/Actors", definition.Id);
            asset.Setup(definition.Id, definition.Name, definition.Role,
                definition.MaxHp, definition.Attack, definition.Defense, reward, skills);
            EditorUtility.SetDirty(asset);

            _actors[definition.Id] = asset;
            return asset;
        }

        private ChapterData CreateChapter(ChapterDefinition definition)
        {
            List<StageEntry> stages = definition.Stages
                .Select(s => new StageEntry(
                    s.Type, s.GrantsSkill, s.ClearReward != null ? GetReward(s.ClearReward) : null))
                .ToList();

            List<ActorData> pool = definition.MonsterPool.Select(GetActor).ToList();
            ActorData boss = definition.Boss != null ? GetActor(definition.Boss) : null;

            ChapterData asset = LoadOrCreate<ChapterData>(Root + "/Chapters", definition.Id);
            asset.Setup(definition.Id, definition.Name, stages, pool, boss);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // 같은 경로에 에셋이 있으면 그것을 재사용합니다. (GUID가 유지되어 다른 에셋과 씬의 연결이 끊기지 않음)
        private static T LoadOrCreate<T>(string folder, string fileName) where T : ScriptableObject
        {
            EnsureFolder(folder);

            string path = $"{folder}/{fileName}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}