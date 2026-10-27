#nullable enable
using System;
using System.Collections.Generic;

namespace CombatTestbed.Presentation.Navigation
{
    /// <summary>
    /// 화면 사이에 허용되는 이동 규칙입니다. (화면 이동 표)
    /// </summary>
    public static class NavigationRules
    {
        private static readonly Dictionary<ScreenId, ScreenId[]> Allowed = new Dictionary<ScreenId, ScreenId[]>
        {
            { ScreenId.Title,       new[] { ScreenId.Story, ScreenId.Battle } },
            { ScreenId.Story,       new[] { ScreenId.StageResult } },
            { ScreenId.Battle,      new[] { ScreenId.StageResult, ScreenId.Defeat } },
            { ScreenId.StageResult, new[] { ScreenId.Story, ScreenId.Battle, ScreenId.Victory } },
            { ScreenId.Victory,     new[] { ScreenId.Title } },
            { ScreenId.Defeat,      new[] { ScreenId.Title } }
        };

        /// <param name="from">현재 화면. 아직 아무 화면도 보여 주지 않았다면 null입니다.</param>
        /// <returns>from에서 to로 이동할 수 있으면 true</returns>
        public static bool CanNavigate(ScreenId? from, ScreenId to)
        {
            // 처음에는 시작 화면으로만 들어갈 수 있음
            if (from == null) return to == ScreenId.Title;

            return Array.IndexOf(Allowed[from.Value], to) >= 0;
        }
    }
}