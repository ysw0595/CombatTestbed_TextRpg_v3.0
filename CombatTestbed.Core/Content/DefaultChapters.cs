using System.Collections.Generic;
using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Core.Content
{
    /// <summary>
    /// 원본 게임의 3개 챕터 구성을 데이터로 옮긴 기본 콘텐츠입니다.
    /// 모든 챕터는 [프롤로그, 전투 x4, 에피소드, 전투 x3, 보스]의 10단계 틀을 따릅니다.
    /// </summary>
    public static class DefaultChapters
    {
        public static readonly ChapterDefinition Chapter1 = new ChapterDefinition(
            "chapter1", "폭풍전야",
            CreateStages(
                episode: new StageDefinition(StageType.Episode, grantsSkill: true),
                boss: new StageDefinition(StageType.Boss, grantsSkill: true, clearReward: DefaultRewards.MoonBlessing)),
            monsterPool: new[] { DefaultActors.Farmer, DefaultActors.Hunter, DefaultActors.Wolf },
            boss: DefaultActors.Priest);

        public static readonly ChapterDefinition Chapter2 = new ChapterDefinition(
            "chapter2", "붉은 숲",
            CreateStages(
                episode: new StageDefinition(StageType.Episode, grantsSkill: true),
                boss: new StageDefinition(StageType.Boss, grantsSkill: true, clearReward: DefaultRewards.MoonBlessing)),
            monsterPool: new[] { DefaultActors.Wolf, DefaultActors.Knight, DefaultActors.Werewolf },
            boss: DefaultActors.VanHelsing);

        public static readonly ChapterDefinition Chapter3 = new ChapterDefinition(
            "chapter3", "적만월(赤滿月)",
            CreateStages(
                episode: new StageDefinition(StageType.Episode, clearReward: DefaultRewards.BloodMoonEnhance),
                boss: new StageDefinition(StageType.Boss)),
            monsterPool: new[] { DefaultActors.Werewolf, DefaultActors.Paladin, DefaultActors.Lycan },
            boss: DefaultActors.WereFenrir);

        /// <summary>
        /// 진행 순서대로의 전체 챕터
        /// </summary>
        public static readonly IReadOnlyList<ChapterDefinition> All = new[] { Chapter1, Chapter2, Chapter3 };

        private static StageDefinition[] CreateStages(StageDefinition episode, StageDefinition boss)
        {
            return new[]
            {
                new StageDefinition(StageType.Prologue),
                new StageDefinition(StageType.Battle),
                new StageDefinition(StageType.Battle),
                new StageDefinition(StageType.Battle),
                new StageDefinition(StageType.Battle),
                episode,
                new StageDefinition(StageType.Battle),
                new StageDefinition(StageType.Battle),
                new StageDefinition(StageType.Battle),
                boss
            };
        }
    }
}