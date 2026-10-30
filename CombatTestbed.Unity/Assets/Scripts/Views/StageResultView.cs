using System.Collections.Generic;
using CombatTestbed.Presentation.Screens;
using UnityEngine;

/// <summary>
/// 단계 결과 화면을 그리는 View입니다. (IStageResultView 구현)
/// 처치 연출 문구가 길어서 스토리 화면처럼 제목 + 스크롤 본문 구조를 씁니다.
/// </summary>
public class StageResultView : TitleBodyViewBase, IStageResultView
{
    public void ShowResult(string title, IReadOnlyList<string> lines)
    {
        // 줄 하나마다 오브젝트를 만들지 않고, 한 텍스트에 줄바꿈으로 합칩니다.
        SetContent(title, string.Join("\n", lines));
    }

    [ContextMenu("Show Test Result")]
    private void ShowTestResult()
    {
        ShowResult("챕터 1 : 폭풍전야 (stage 2) 클리어", new[]
        {
            "농부의 목이 꺾였다. 처치 문구가 들어가는 자리입니다.",
            "",
            "최대 체력이 1 증가했다."
        });
    }
}