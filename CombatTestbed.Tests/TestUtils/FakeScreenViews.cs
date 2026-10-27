using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Screens;

namespace CombatTestbed.Tests.TestUtils
{
    public class FakeTitleView : ITitleView
    {
        public string Title { get; private set; } = "";
        public string Subtitle { get; private set; } = "";
        public int ShowCount { get; private set; }

        public void ShowTitle(string title, string subtitle)
        {
            Title = title;
            Subtitle = subtitle;
            ShowCount++;
        }
    }

    public class FakeStoryView : IStoryView
    {
        public string Title { get; private set; } = "";
        public string Body { get; private set; } = "";
        public int ShowCount { get; private set; }

        public void ShowStory(string title, string body)
        {
            Title = title;
            Body = body;
            ShowCount++;
        }
    }

    public class FakeStageResultView : IStageResultView
    {
        public string Title { get; private set; } = "";
        public IReadOnlyList<string> Lines { get; private set; } = new List<string>();
        public int ShowCount { get; private set; }

        public void ShowResult(string title, IReadOnlyList<string> lines)
        {
            Title = title;
            Lines = new List<string>(lines);
            ShowCount++;
        }
    }

    public class FakeEndingView : IEndingView
    {
        public string Title { get; private set; } = "";
        public string Body { get; private set; } = "";
        public int ShowCount { get; private set; }

        public void ShowEnding(string title, string body)
        {
            Title = title;
            Body = body;
            ShowCount++;
        }
    }

    /// <summary>
    /// 스토리 문구 공급자의 가짜입니다. 문구에 단계/결과 정보를 담아서 어떤 값으로 요청했는지 확인할 수 있게 합니다.
    /// </summary>
    public class FakeStoryTextProvider : IStoryTextProvider
    {
        public string GetStageText(StageInfo stage)
        {
            return $"story:{stage.Chapter.Id}:{stage.StageIndex}";
        }

        public string GetEndingText(CampaignResult result, StageInfo lastStage)
        {
            return $"ending:{result}:{lastStage.Chapter.Id}";
        }
    }
}