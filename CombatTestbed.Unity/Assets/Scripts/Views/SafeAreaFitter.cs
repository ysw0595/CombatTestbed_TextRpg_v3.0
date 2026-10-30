using UnityEngine;

/// <summary>
/// 화면의 안전 영역(노치, 둥근 모서리, 홈 바를 피한 영역)에 맞춰 이 RectTransform의 크기를 조절합니다.
/// 모든 UI를 이 오브젝트의 자식으로 두면 기기에 상관없이 가려지는 일이 없습니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        // 화면 회전이나 해상도 변경으로 값이 달라졌을 때만 다시 계산
        if (Screen.safeArea != _lastSafeArea ||
            Screen.width != _lastScreenSize.x ||
            Screen.height != _lastScreenSize.y)
        {
            Apply();
        }
    }

    private void Apply()
    {
        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0) return;

        // 안전 영역을 화면 대비 0~1 비율(앵커)로 바꿔서 적용
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}