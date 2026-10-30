using System.Collections.Generic;
using CombatTestbed.Presentation.Screens;
using TMPro;
using UnityEngine;

/// <summary>
/// 단계 결과 화면을 그리는 View입니다. (IStageResultView 구현)
/// </summary>
public class StageResultView : MonoBehaviour, IStageResultView
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _linesText;

    public void ShowResult(string title, IReadOnlyList<string> lines)
    {
        _titleText.text = title;

        // 보상 줄은 몇 개 안 되므로 줄 하나마다 오브젝트를 만들지 않고 한 텍스트에 줄바꿈으로 합칩니다.
        _linesText.text = string.Join("\n", lines);
    }

    private void Reset()
    {
        _titleText = ViewWiring.Find<TMP_Text>(transform, "TitleText");
        _linesText = ViewWiring.Find<TMP_Text>(transform, "LinesText");
    }

    [ContextMenu("Show Test Result")]
    private void ShowTestResult()
    {
        ShowResult("스테이지 1 클리어", new[]
        {
            "Blood_Explosion을(를) 배웠습니다.",
            "체력을 20 회복했습니다.",
            "공격력이 1 증가했습니다."
        });
    }
}