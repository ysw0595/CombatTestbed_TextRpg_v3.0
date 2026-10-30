using System.Collections.Generic;
using System.Text;
using CombatTestbed.Presentation.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 화면 전체를 그리는 View입니다. (IBattleView 구현)
/// 무엇을 보여 줄지는 BattlePresenter가 정하고, 이 클래스는 받은 값을 uGUI에 그리기만 합니다.
/// </summary>
public class BattleView : MonoBehaviour, IBattleView
{
    [Header("헤더")]
    [SerializeField] private TMP_Text _headerText;

    [Header("액터 패널 (위: 몬스터, 아래: 플레이어)")]
    [SerializeField] private ActorPanelView _enemyPanel;
    [SerializeField] private ActorPanelView _playerPanel;

    [Header("전투 로그")]
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private ScrollRect _logScrollRect;

    [Tooltip("로그에 보관할 최대 줄 수입니다. 넘으면 오래된 줄부터 지웁니다.")]
    [SerializeField, Min(1)] private int _maxLogLines = 100;

    [Tooltip("새 줄이 추가될 때 자동으로 맨 아래로 스크롤합니다.")]
    [SerializeField] private bool _autoScrollToBottom = true;

    private readonly List<string> _logLines = new List<string>();
    private readonly StringBuilder _logBuilder = new StringBuilder();

    public void SetHeader(string text)
    {
        _headerText.text = text;
    }

    public void ShowActor(ActorSide side, ActorViewModel actor)
    {
        ActorPanelView panel = side == ActorSide.Player ? _playerPanel : _enemyPanel;
        panel.Show(actor);
    }

    public void AppendLog(string line)
    {
        _logLines.Add(line);

        // 한도를 넘으면 가장 오래된 줄부터 제거 (한 번에 한 줄씩 추가되므로 보통 1회만 실행)
        while (_logLines.Count > _maxLogLines)
        {
            _logLines.RemoveAt(0);
        }

        RefreshLogText();

        if (_autoScrollToBottom)
        {
            ScrollToBottom();
        }
    }

    public void ClearLog()
    {
        _logLines.Clear();
        RefreshLogText();
    }

    private void RefreshLogText()
    {
        _logBuilder.Clear();
        for (int i = 0; i < _logLines.Count; i++)
        {
            if (i > 0) _logBuilder.Append('\n');
            _logBuilder.Append(_logLines[i]);
        }

        _logText.text = _logBuilder.ToString();
    }

    private void ScrollToBottom()
    {
        // 텍스트가 바뀐 직후에는 Content의 높이가 아직 옛날 값이라서,
        // 레이아웃을 먼저 강제로 다시 계산한 뒤에 스크롤 위치를 맞춥니다.
        Canvas.ForceUpdateCanvases();
        _logScrollRect.verticalNormalizedPosition = 0f; // 0 = 맨 아래, 1 = 맨 위
    }

    // 컴포넌트를 처음 붙일 때나 Reset 메뉴를 누를 때, 자식 이름으로 연결을 자동으로 채웁니다.
    private void Reset()
    {
        _headerText = FindComponent<TMP_Text>("HeaderText");
        _enemyPanel = FindComponent<ActorPanelView>("EnemyArea");
        _playerPanel = FindComponent<ActorPanelView>("PlayerArea");
        _logScrollRect = FindComponent<ScrollRect>("LogScrollView");
        _logText = FindComponent<TMP_Text>("LogScrollView/Viewport/Content/LogText");
    }

    private T FindComponent<T>(string path) where T : Component
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<T>() : null;
    }

    // ---- 에디터에서 손으로 확인하기 위한 메뉴 (Inspector의 점 세 개 메뉴) ----

    private int _testLogCount;

    [ContextMenu("Test/Show Test Battle")]
    private void ShowTestBattle()
    {
        SetHeader("챕터 1 : 폭풍전야 (stage 1) - 라운드 1");
        ShowActor(ActorSide.Enemy, new ActorViewModel("고블린", 18, 30, 12, new[] { "중독 3", "출혈 2" }));
        ShowActor(ActorSide.Player, new ActorViewModel("용사", 70, 100, 20, new string[0]));
    }

    [ContextMenu("Test/Append Test Log")]
    private void AppendTestLog()
    {
        _testLogCount++;
        AppendLog($"테스트 로그 {_testLogCount}");
    }

    [ContextMenu("Test/Clear Log")]
    private void ClearTestLog()
    {
        _testLogCount = 0;
        ClearLog();
    }
}
