using CombatTestbed.Presentation.Screens;
using TMPro;
using UnityEngine;

/// <summary>
/// 타이틀 화면을 그리는 View입니다. (ITitleView 구현)
/// </summary>
public class TitleView : MonoBehaviour, ITitleView
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;

    public void ShowTitle(string title, string subtitle)
    {
        _titleText.text = title;
        _subtitleText.text = subtitle;
    }

    private void Reset()
    {
        _titleText = ViewWiring.Find<TMP_Text>(transform, "TitleText");
        _subtitleText = ViewWiring.Find<TMP_Text>(transform, "SubtitleText");
    }

    [ContextMenu("Show Test Title")]
    private void ShowTestTitle()
    {
        ShowTitle("IF THE NIGHT COMES", "밤이 오면");
    }
}