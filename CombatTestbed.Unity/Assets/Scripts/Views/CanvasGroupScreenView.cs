using CombatTestbed.Presentation.Navigation;
using UnityEngine;

/// <summary>
/// 화면 패널 하나를 CanvasGroup으로 보이거나 숨기는 View입니다.
/// (알파 0 + 입력 차단 방식이라 오브젝트는 계속 활성 상태로 둡니다.)
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupScreenView : MonoBehaviour, IScreenView
{
    private CanvasGroup _canvasGroup;

    private CanvasGroup Group
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            return _canvasGroup;
        }
    }

    private void Awake()
    {
        // 처음에는 모든 화면을 숨긴 상태로 시작 (처음 보일 화면은 ScreenNavigator가 켬)
        Hide();
    }

    [ContextMenu("Show")]
    public void Show()
    {
        SetVisible(true);
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        Group.alpha = visible ? 1f : 0f;
        Group.interactable = visible;
        Group.blocksRaycasts = visible;
    }
}