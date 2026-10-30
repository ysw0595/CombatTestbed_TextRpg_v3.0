using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 키보드 숫자 키(1~5)를 선택 버튼 슬롯 번호로 바꿔 ChoiceButtonsView에 전달합니다.
/// 입력을 읽는 일만 하고, 선택이 유효한지는 ChoicePresenter가 판단합니다.
/// </summary>
[RequireComponent(typeof(ChoiceButtonsView))]
public class ChoiceKeyboardInput : MonoBehaviour
{
    [Header("슬롯 1~5에 대응하는 키 (배열 순서 = 슬롯 번호)")]
    [SerializeField]
    private Key[] _primaryKeys =
        { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };

    [Header("보조 키 (숫자 패드)")]
    [SerializeField]
    private Key[] _secondaryKeys =
        { Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5 };

    private ChoiceButtonsView _buttons;

    private void Awake()
    {
        _buttons = GetComponent<ChoiceButtonsView>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return; // 키보드가 없는 기기(모바일 등)에서는 아무것도 하지 않음

        for (int slot = 0; slot < _primaryKeys.Length; slot++)
        {
            if (WasPressed(keyboard, _primaryKeys, slot) || WasPressed(keyboard, _secondaryKeys, slot))
            {
                _buttons.SelectSlot(slot);
                return; // 한 프레임에 한 번만 선택 (여러 키를 동시에 눌러도 가장 앞 슬롯만)
            }
        }
    }

    private static bool WasPressed(Keyboard keyboard, Key[] keys, int slot)
    {
        return slot < keys.Length && keyboard[keys[slot]].wasPressedThisFrame;
    }
}