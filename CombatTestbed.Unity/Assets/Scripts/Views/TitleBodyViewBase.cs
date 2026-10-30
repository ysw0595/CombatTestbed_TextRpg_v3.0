using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "제목 + 긴 본문(스크롤)" 구조의 화면이 공유하는 기반 클래스입니다.
/// 스토리 화면과 엔딩 화면이 같은 모양이라 코드를 한 번만 씁니다.
/// </summary>
public abstract class TitleBodyViewBase : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private ScrollRect _bodyScrollRect;

    protected void SetContent(string title, string body)
    {
        _titleText.text = title;
        _bodyText.text = body;

        // 새 내용이 들어오면 항상 맨 위부터 읽도록 스크롤을 되돌립니다.
        // (높이 계산이 끝난 뒤에 맞춰야 해서 레이아웃을 먼저 갱신합니다.)
        Canvas.ForceUpdateCanvases();
        _bodyScrollRect.verticalNormalizedPosition = 1f; // 1 = 맨 위
    }

    private void Reset()
    {
        _titleText = ViewWiring.Find<TMP_Text>(transform, "TitleText");
        _bodyText = ViewWiring.Find<TMP_Text>(transform, "BodyScrollView/Viewport/Content/BodyText");
        _bodyScrollRect = ViewWiring.Find<ScrollRect>(transform, "BodyScrollView");
    }
}