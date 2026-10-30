using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Presentation.Battle;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Game;
using CombatTestbed.Presentation.Navigation;
using CombatTestbed.Presentation.Screens;
using UnityEngine;

/// <summary>
/// 컴포지션 루트: 게임에 필요한 객체를 한 곳에서 만들고 서로 연결합니다.
/// 여기서만 구체 클래스를 알고, 다른 코드는 인터페이스로만 서로를 압니다.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("화면 View (각 화면 패널에 붙어 있는 컴포넌트)")]
    [SerializeField] private TitleView _titleView;
    [SerializeField] private StoryView _storyView;
    [SerializeField] private BattleView _battleView;
    [SerializeField] private StageResultView _stageResultView;
    [SerializeField] private EndingView _endingView;

    [Header("선택 버튼")]
    [SerializeField] private ChoiceButtonsView _choiceButtons;

    [Header("캠페인 데이터")]
    [Tooltip("한 판의 구성(플레이어, 스킬, 챕터, 몬스터 등)을 담은 데이터 에셋입니다.")]
    [SerializeField] private CampaignData _campaignData;

    [Header("시나리오 텍스트")]
    [Tooltip("Resources 폴더 안에서 시나리오 텍스트 파일을 찾을 폴더 이름입니다.")]
    [SerializeField] private string _scenarioFolder = "Scenario";

    [Header("난수")]
    [Tooltip("0이면 매번 다른 전개, 0이 아니면 같은 값으로 항상 같은 전개(재현 테스트용)입니다.")]
    [SerializeField] private int _randomSeed = 0;

    private GameController _game;

    // Awake가 아니라 Start에서 조립합니다.
    // 모든 오브젝트의 Awake(버튼 연결, 화면 숨김)가 끝난 뒤에 조립해야 안전하기 때문입니다.
    private void Start()
    {
        if (_campaignData == null)
        {
            Debug.LogError("[GameBootstrapper] Campaign Data가 연결되지 않았습니다. " +
                           "메뉴 Combat Testbed > Generate Default Data를 실행한 뒤 연결해 주세요.", this);
            return;
        }

        IRandomSource random = _randomSeed == 0
            ? new SystemRandomSource()
            : new SystemRandomSource(_randomSeed);

        // 화면 패널: 각 View와 같은 오브젝트에 붙은 CanvasGroupScreenView를 가져옴
        var screens = new Dictionary<ScreenId, IScreenView>
        {
            { ScreenId.Title, _titleView.GetComponent<CanvasGroupScreenView>() },
            { ScreenId.Story, _storyView.GetComponent<CanvasGroupScreenView>() },
            { ScreenId.Battle, _battleView.GetComponent<CanvasGroupScreenView>() },
            { ScreenId.StageResult, _stageResultView.GetComponent<CanvasGroupScreenView>() },
            { ScreenId.Victory, _endingView.GetComponent<CanvasGroupScreenView>() }, // 클리어와 게임 오버는
            { ScreenId.Defeat, _endingView.GetComponent<CanvasGroupScreenView>() }   // 같은 화면 패널을 씁니다
        };

        var navigator = new ScreenNavigator(screens);
        var flow = new ScreenFlowPresenter(navigator);
        var choices = new ChoicePresenter(_choiceButtons);
        var texts = new ScenarioTextProvider(LoadScenarioText);

        var startChoices = new[]
 {
            new ActionChoice(_campaignData.PlayerId, _campaignData.PlayerName)
        };

        var title = new TitlePresenter(_titleView, choices, startChoices);
        var story = new StoryPresenter(_storyView, choices, texts); 
        var battle = new BattlePresenter(_battleView, choices, texts);
        var result = new StageResultPresenter(_stageResultView, choices, texts);
        var ending = new EndingPresenter(_endingView, choices, texts);

        _game = new GameController(
            navigator, flow, choices, title, story, battle, result, ending,
            characterId => CreateCampaign(characterId, random));

        _game.QuitRequested += OnQuitRequested;
        _game.Start();
    }

    private CampaignManager CreateCampaign(string characterId, IRandomSource random)
    {
        if (characterId == _campaignData.PlayerId)
        {
            return _campaignData.CreateCampaign(random);
        }

        throw new ArgumentException($"알 수 없는 캐릭터입니다: {characterId}", nameof(characterId));
    }

    // Resources/Scenario/{key}.txt 를 읽어 문구를 돌려줍니다. 파일이 없으면 null (공급자가 대체 문구를 사용)
    private string LoadScenarioText(string key)
    {
        TextAsset asset = Resources.Load<TextAsset>($"{_scenarioFolder}/{key}");
        return asset != null ? asset.text : null;
    }

    private void OnQuitRequested()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (_game == null) return;

        _game.QuitRequested -= OnQuitRequested;
        _game.Dispose(); // 모든 Presenter의 이벤트 구독을 함께 해제
    }

    // 컴포넌트를 처음 붙일 때나 Reset 메뉴를 누를 때, 씬에서 각 View를 찾아 자동으로 연결합니다.
    private void Reset()
    {
        _titleView = FindFirstObjectByType<TitleView>(FindObjectsInactive.Include);
        _storyView = FindFirstObjectByType<StoryView>(FindObjectsInactive.Include);
        _battleView = FindFirstObjectByType<BattleView>(FindObjectsInactive.Include);
        _stageResultView = FindFirstObjectByType<StageResultView>(FindObjectsInactive.Include);
        _endingView = FindFirstObjectByType<EndingView>(FindObjectsInactive.Include);
        _choiceButtons = FindFirstObjectByType<ChoiceButtonsView>(FindObjectsInactive.Include);

#if UNITY_EDITOR
        _campaignData = UnityEditor.AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/Data/DefaultCampaign.asset");
#endif
    }
}