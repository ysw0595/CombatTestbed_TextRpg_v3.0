using CombatTestbed.Presentation.Battle;
using TMPro;
using UnityEngine;

/// <summary>
/// 액터 한 명(몬스터 또는 플레이어)의 정보를 보여 주는 패널입니다.
/// 화면에 그리는 일만 하고, 무엇을 보여 줄지는 Presenter가 ActorViewModel로 넘겨 줍니다.
/// </summary>
public class ActorPanelView : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _attackText;
    [SerializeField] private TMP_Text _statusText;

    [Header("체력 바의 채움 영역 (배경 안쪽, 앵커로 폭을 조절)")]
    [SerializeField] private RectTransform _hpFill;

    public void Show(ActorViewModel actor)
    {
        _nameText.text = actor.Name;
        _hpText.text = $"HP {actor.CurrentHp} / {actor.MaxHp}";
        _attackText.text = $"공격력 {actor.Attack}";
        _statusText.text = actor.Statuses.Count > 0 ? string.Join("   ", actor.Statuses) : string.Empty;

        // 채움 영역의 오른쪽 끝 앵커를 체력 비율만큼 옮겨서 폭을 조절 (스프라이트가 필요 없는 방식)
        _hpFill.anchorMin = Vector2.zero;
        _hpFill.anchorMax = new Vector2(actor.HpRatio, 1f);
        _hpFill.offsetMin = Vector2.zero;
        _hpFill.offsetMax = Vector2.zero;
    }

    // 컴포넌트를 처음 붙일 때나 Reset 메뉴를 누를 때, 자식 이름으로 연결을 자동으로 채움
    private void Reset()
    {
        _nameText = FindText("NameText");
        _hpText = FindText("HpText");
        _attackText = FindText("AttackText");
        _statusText = FindText("StatusText");

        Transform fill = transform.Find("HpBar/HpFill");
        _hpFill = fill != null ? fill.GetComponent<RectTransform>() : null;
    }

    private TMP_Text FindText(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    // 에디터에서 손으로 모양을 확인하기 위한 메뉴 (Inspector의 점 세 개 메뉴)
    [ContextMenu("Show Test Actor")]
    private void ShowTestActor()
    {
        Show(new ActorViewModel("테스트 몬스터", 18, 30, 12, new[] { "중독 3", "출혈 2" }));
    }
}