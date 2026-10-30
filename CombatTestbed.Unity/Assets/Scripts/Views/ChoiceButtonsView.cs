using System;
using System.Collections.Generic;
using CombatTestbed.Presentation.Choices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모든 화면이 공유하는 선택 버튼 5개의 View입니다.
/// 버튼 클릭을 슬롯 번호(0부터)로 바꿔 SlotSelected로 알립니다. 키보드 입력도 SelectSlot으로 같은 경로를 씁니다.
/// </summary>
public class ChoiceButtonsView : MonoBehaviour, IChoiceView
{
    [Header("선택 버튼 (왼쪽 위부터 가로 순서 = 키보드 1~5번)")]
    [SerializeField] private Button[] _buttons = new Button[ChoicePresenter.MaxChoices];

    private TMP_Text[] _labels;
    private bool _testHandlerAttached;

    public event Action<int> SlotSelected;

    private void Awake()
    {
        _labels = new TMP_Text[_buttons.Length];

        for (int i = 0; i < _buttons.Length; i++)
        {
            int slotIndex = i; // 람다가 반복이 끝난 뒤의 i가 아니라 지금 값을 기억하도록 복사
            _labels[i] = _buttons[i].GetComponentInChildren<TMP_Text>();
            _buttons[i].onClick.AddListener(() => SelectSlot(slotIndex));
        }

        // 처음에는 모든 버튼을 숨김 (선택지는 Presenter가 주입함)
        ShowChoices(new List<ActionChoice>());
    }

    public void ShowChoices(IReadOnlyList<ActionChoice> choices)
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            bool hasChoice = i < choices.Count;
            _buttons[i].gameObject.SetActive(hasChoice);

            if (!hasChoice) continue;

            _labels[i].text = $"[{i + 1}] {choices[i].Label}";
            _buttons[i].interactable = choices[i].IsEnabled;
        }
    }

    /// <summary>
    /// 슬롯을 선택한 것으로 알립니다. (버튼 클릭과 키보드 입력이 함께 사용)
    /// </summary>
    public void SelectSlot(int slotIndex)
    {
        SlotSelected?.Invoke(slotIndex);
    }

    // 에디터에서 손으로 동작을 확인하기 위한 메뉴 (Inspector의 점 세 개 메뉴)
    [ContextMenu("Show Test Choices")]
    private void ShowTestChoices()
    {
        if (!_testHandlerAttached)
        {
            _testHandlerAttached = true;
            SlotSelected += slot => Debug.Log($"슬롯 {slot} 선택됨 (키보드 {slot + 1}번)");
        }

        ShowChoices(new List<ActionChoice>
        {
            new ActionChoice("a", "첫 번째"),
            new ActionChoice("b", "두 번째"),
            new ActionChoice("c", "세 번째 (비활성)", false),
            new ActionChoice("d", "네 번째"),
            new ActionChoice("e", "다섯 번째 선택지입니다")
        });
    }

    private void OnDestroy()
    {
        foreach (Button button in _buttons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}