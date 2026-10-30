using System.Text;
using CombatTestbed.Presentation.Screens;
using UnityEngine;

/// <summary>
/// 스토리 화면을 그리는 View입니다. (IStoryView 구현)
/// </summary>
public class StoryView : TitleBodyViewBase, IStoryView
{
    public void ShowStory(string title, string body)
    {
        SetContent(title, body);
    }

    [ContextMenu("Show Test Story")]
    private void ShowTestStory()
    {
        var builder = new StringBuilder();
        for (int i = 1; i <= 12; i++)
        {
            builder.Append($"{i}번째 문단입니다. 스크롤과 줄바꿈이 잘 되는지 확인하기 위한 테스트 문장입니다.\n\n");
        }

        ShowStory("챕터 1 : 폭풍전야", builder.ToString());
    }
}