using System.Text;
using CombatTestbed.Presentation.Screens;
using UnityEngine;

/// <summary>
/// 엔딩 화면을 그리는 View입니다. (IEndingView 구현)
/// </summary>
public class EndingView : TitleBodyViewBase, IEndingView
{
    public void ShowEnding(string title, string body)
    {
        SetContent(title, body);
    }

    [ContextMenu("Show Test Ending")]
    private void ShowTestEnding()
    {
        var builder = new StringBuilder();
        for (int i = 1; i <= 12; i++)
        {
            builder.Append($"{i}번째 엔딩 문단입니다. 스크롤과 줄바꿈이 잘 되는지 확인하기 위한 테스트 문장입니다.\n\n");
        }

        ShowEnding("게임 클리어", builder.ToString());
    }
}