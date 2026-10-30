#nullable enable
using System;
using CombatTestbed.Core.Campaign;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 스토리, 엔딩, 전투 연출 문구를 "시나리오 키"로 바꿔 불러오는 공급자입니다.
    /// 문구를 실제로 어디서 읽어 올지(Unity의 Resources 등)는 생성자로 받은 loader가 정합니다.
    /// </summary>
    public sealed class ScenarioTextProvider : IStoryTextProvider, IEncounterTextProvider
    {
        private readonly Func<string, string?> _loader;

        /// <param name="loader">시나리오 키로 문구를 읽어 오는 함수. 문구가 없으면 null을 돌려줍니다.</param>
        public ScenarioTextProvider(Func<string, string?> loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        // ---- 스토리/엔딩: 꼭 필요한 문구라서, 없으면 빠진 키를 알려 주는 대체 문구를 보여 줍니다. ----

        public string GetStageText(StageInfo stage)
        {
            string kind;
            switch (stage.Stage.Type)
            {
                case StageType.Prologue:
                    kind = "Prologue";
                    break;
                case StageType.Episode:
                    kind = "Episode";
                    break;
                default:
                    throw new ArgumentException($"스토리 단계가 아닙니다: {stage.Stage.Type}", nameof(stage));
            }

            return LoadRequired($"Chapter{stage.ChapterIndex + 1}_{kind}");
        }

        public string GetEndingText(CampaignResult result, StageInfo lastStage)
        {
            return LoadRequired(EndingKey(result, lastStage));
        }

        // ---- 전투 연출: 있으면 좋은 문구라서, 없으면 null입니다. ----

        public string? GetAppearText(string actorId)
        {
            return LoadOptional($"Appear_{RequireId(actorId)}");
        }

        public string? GetSlainText(string actorId)
        {
            return LoadOptional($"Slain_{RequireId(actorId)}");
        }

        // ---- 내부 ----

        private static string EndingKey(CampaignResult result, StageInfo lastStage)
        {
            if (result == CampaignResult.Victory) return "Victory";

            // 보스 단계에서 졌다면 그 보스 전용 문구, 아니면 일반 몬스터에게 진 문구
            string? bossId = lastStage.Stage.Type == StageType.Boss ? lastStage.Chapter.Boss?.Id : null;
            return bossId != null ? $"Defeat_{bossId}" : "Defeat_Mob";
        }

        private static string RequireId(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException("액터 Id는 비어 있을 수 없습니다.", nameof(actorId));
            }

            return actorId;
        }

        private string LoadRequired(string key)
        {
            return LoadOptional(key) ?? $"[스토리 텍스트 없음: {key}]";
        }

        private string? LoadOptional(string key)
        {
            string? text = _loader(key);
            if (text == null) return null;

            // 줄바꿈을 \n으로 통일하고, 파일 앞뒤의 빈 줄을 제거합니다.
            text = text.Replace("\r\n", "\n").Trim();

            return text.Length == 0 ? null : text;
        }
    }
}