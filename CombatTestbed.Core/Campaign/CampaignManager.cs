#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign.States;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Pipeline;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Campaign
{
    /// <summary>
    /// 챕터/단계 진행 전체를 관리하는 캠페인 관리자입니다. (바깥 FSM, 전투 하나는 TurnManager가 담당)
    /// 입력은 Start / Continue / UseSkill 세 가지이고, 진행 상황은 이벤트로 알립니다.
    /// (이벤트 핸들러는 읽기 전용 관찰자여야 합니다.)
    /// </summary>
    public sealed class CampaignManager
    {
        private readonly List<ChapterDefinition> _chapters;
        private readonly IReadOnlyList<SkillDefinition> _skillPool;
        private readonly IRandomSource _random;

        private readonly StateMachine<ICampaignState> _machine = new StateMachine<ICampaignState>();
        private readonly ICampaignState _storyState;
        private readonly ICampaignState _battleState;
        private readonly ICampaignState _resultState;
        private readonly ICampaignState _victoryState;
        private readonly ICampaignState _defeatState;

        private MonsterRotation? _rotation;
        private ActorDefinition? _currentEnemyDefinition;
        private StageClearResult? _pendingResult;

        public BattleActor Player { get; }

        /// <summary>
        /// 플레이어가 배운 스킬 목록 (스킬 버튼 구성에 사용)
        /// </summary>
        public SkillBook SkillBook { get; }

        public CampaignPhase Phase => _machine.Current?.Phase ?? CampaignPhase.NotStarted;

        public int ChapterIndex { get; private set; }
        public int StageIndex { get; private set; }
        public ChapterDefinition CurrentChapter => _chapters[ChapterIndex];
        public StageDefinition CurrentStage => CurrentChapter.Stages[StageIndex];

        /// <summary>
        /// 진행 중이거나 가장 최근의 전투. 전투를 한 번도 시작하지 않았다면 null입니다.
        /// </summary>
        public TurnManager? CurrentBattle { get; private set; }

        public BattleActor? CurrentEnemy => CurrentBattle?.Monster;

        /// <summary>
        /// 캠페인 단계가 바뀔 때 발행됩니다. (이전, 새 단계) 새 단계에서 일어나는 일의 이벤트보다 먼저 발행됩니다.
        /// </summary>
        public event Action<CampaignPhase, CampaignPhase>? PhaseChanged;

        /// <summary>
        /// 새 단계가 시작될 때 발행됩니다. (스토리 선택, 챕터 제목 표시에 사용)
        /// </summary>
        public event Action<StageInfo>? StageStarted;

        /// <summary>
        /// 전투가 시작될 때 발행됩니다. 화면은 여기서 받은 TurnManager의 이벤트를 구독하고,
        /// 전투가 끝나면 구독을 해제합니다.
        /// </summary>
        public event Action<TurnManager>? BattleStarted;

        /// <summary>
        /// 단계를 마치고 결과를 보여 줄 때 발행됩니다.
        /// </summary>
        public event Action<StageClearResult>? StageCleared;

        /// <summary>
        /// 캠페인이 끝났을 때 한 번 발행됩니다.
        /// </summary>
        public event Action<CampaignResult>? CampaignEnded;

        /// <param name="startingSkills">처음부터 배우고 있는 스킬 (1개 이상)</param>
        /// <param name="learnableSkillPool">진행 중에 무작위로 배울 수 있는 스킬 풀</param>
        public CampaignManager(
            IEnumerable<ChapterDefinition> chapters,
            ActorDefinition playerDefinition,
            IEnumerable<SkillDefinition> startingSkills,
            IEnumerable<SkillDefinition> learnableSkillPool,
            IRandomSource random)
        {
            if (chapters == null) throw new ArgumentNullException(nameof(chapters));
            if (playerDefinition == null) throw new ArgumentNullException(nameof(playerDefinition));
            if (startingSkills == null) throw new ArgumentNullException(nameof(startingSkills));
            if (learnableSkillPool == null) throw new ArgumentNullException(nameof(learnableSkillPool));
            _random = random ?? throw new ArgumentNullException(nameof(random));

            _chapters = new List<ChapterDefinition>(chapters);

            if (_chapters.Count == 0)
            {
                throw new ArgumentException("챕터가 최소 1개 필요합니다.", nameof(chapters));
            }

            if (_chapters.Contains(null!))
            {
                throw new ArgumentException("챕터 목록에 null이 있을 수 없습니다.", nameof(chapters));
            }

            var starting = new List<SkillDefinition>(startingSkills);

            if (starting.Count == 0)
            {
                throw new ArgumentException("시작 스킬이 최소 1개 필요합니다.", nameof(startingSkills));
            }

            _skillPool = new List<SkillDefinition>(learnableSkillPool).AsReadOnly();

            Player = ActorFactory.Create(playerDefinition);
            SkillBook = new SkillBook();
            foreach (SkillDefinition skill in starting)
            {
                SkillBook.Learn(skill);
            }

            _storyState = new StoryStageState(this);
            _battleState = new BattleStageState(this);
            _resultState = new StageResultState(this);
            _victoryState = new VictoryState(this);
            _defeatState = new DefeatState(this);

            // 상태 머신의 전환 통보를 공개용 단계(CampaignPhase)로 변환해 외부에 전달
            _machine.StateChanged += (previous, next) =>
                PhaseChanged?.Invoke(previous?.Phase ?? CampaignPhase.NotStarted, next.Phase);
        }

        /// <summary>
        /// 캠페인을 시작해 첫 챕터의 첫 단계로 들어갑니다.
        /// </summary>
        public void Start()
        {
            if (_machine.Current != null)
            {
                throw new InvalidOperationException("이미 시작된 캠페인입니다.");
            }

            ChapterIndex = 0;
            StageIndex = 0;
            PrepareChapter();
            BeginStage();
        }

        /// <summary>
        /// "다음" 입력. 스토리 단계와 단계 결과 단계에서만 사용할 수 있습니다.
        /// </summary>
        public void Continue()
        {
            EnsureStarted().Continue();
        }

        /// <summary>
        /// 스킬 사용 입력. 전투 단계에서만 사용할 수 있고, 배운 스킬이어야 합니다.
        /// </summary>
        public void UseSkill(SkillDefinition skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            EnsureStarted().UseSkill(skill);
        }

        private ICampaignState EnsureStarted()
        {
            return _machine.Current
                ?? throw new InvalidOperationException("캠페인이 시작되지 않았습니다. Start()를 먼저 호출하세요.");
        }

        // ---- 아래는 같은 어셈블리의 캠페인 상태 클래스들이 사용하는 내부 동작입니다. ----

        internal void NotifyStageStarted()
        {
            StageStarted?.Invoke(CurrentStageInfo());
        }

        internal void NotifyStageCleared()
        {
            StageCleared?.Invoke(_pendingResult!);
        }

        internal void NotifyCampaignEnded(CampaignResult result)
        {
            CampaignEnded?.Invoke(result);
        }

        /// <summary>
        /// 새 챕터에 들어갈 때 일반 몬스터 순환기를 준비합니다. (시작 위치만 난수)
        /// </summary>
        private void PrepareChapter()
        {
            _rotation = CurrentChapter.MonsterPool.Count > 0
                ? new MonsterRotation(CurrentChapter.MonsterPool, _random)
                : null;
        }

        /// <summary>
        /// 현재 단계의 종류에 맞는 상태로 들어갑니다.
        /// </summary>
        private void BeginStage()
        {
            StageType type = CurrentStage.Type;

            if (type == StageType.Battle || type == StageType.Boss)
            {
                _machine.ChangeState(_battleState);
            }
            else
            {
                _machine.ChangeState(_storyState);
            }
        }

        internal void StartBattle()
        {
            ActorDefinition enemyDefinition = CurrentStage.Type == StageType.Boss
                ? CurrentChapter.Boss!
                : _rotation!.Next();

            _currentEnemyDefinition = enemyDefinition;

            BattleActor enemy = ActorFactory.Create(enemyDefinition);
            var selector = ActorFactory.CreateSelector(enemyDefinition, ChooseBehaviorStartIndex(enemyDefinition));

            CurrentBattle = new TurnManager(Player, enemy, new ActionPipeline(), selector);

            BattleStarted?.Invoke(CurrentBattle);
            CurrentBattle.Start();
            CheckBattleFinished();
        }

        /// <summary>
        /// 스킬을 순환하는 몬스터(보스)의 시작 스킬 순번을 정합니다.
        /// 마지막 스킬은 시작 후보에서 제외합니다. (원본: 스킬 3개 중 앞 2개 중에서 무작위)
        /// </summary>
        private int ChooseBehaviorStartIndex(ActorDefinition enemyDefinition)
        {
            int skillCount = enemyDefinition.BehaviorSkills.Count;

            if (skillCount == 0) return 0;

            return _random.Next(0, Math.Max(1, skillCount - 1));
        }

        internal void SubmitSkill(SkillDefinition skill)
        {
            TurnManager battle = CurrentBattle!;

            battle.SubmitPlayerAction(new SkillAction(skill, Player, battle.Monster));
            CheckBattleFinished();
        }

        /// <summary>
        /// 전투가 끝났는지 확인하고, 끝났다면 승패에 따라 다음 상태로 보냅니다.
        /// (TurnManager의 이벤트에 반응하지 않고, 호출 직후에 명시적으로 확인합니다.)
        /// </summary>
        private void CheckBattleFinished()
        {
            TurnManager battle = CurrentBattle!;

            if (!battle.IsBattleOver) return;

            if (Player.Health.IsDead)
            {
                _machine.ChangeState(_defeatState);
                return;
            }

            // 승리: 몬스터 처치 보상 -> 전투 중 상태이상 정리 -> 단계 클리어 처리
            var rewards = new List<EffectOutcome>();
            RewardDefinition? monsterReward = _currentEnemyDefinition!.Reward;

            if (monsterReward != null)
            {
                rewards.AddRange(RewardApplier.Apply(monsterReward, Player));
            }

            Player.StatusEffects.Clear(Player);

            CompleteStage(rewards);
        }

        /// <summary>
        /// 단계를 마치고 클리어 규칙(스킬 획득, 클리어 보상)을 적용한 뒤 결과 단계로 넘어갑니다.
        /// </summary>
        internal void CompleteStage(IEnumerable<EffectOutcome> battleRewards)
        {
            var rewards = new List<EffectOutcome>(battleRewards);
            StageDefinition stage = CurrentStage;
            SkillDefinition? learned = null;

            // 원본 순서: 스킬 획득 -> 체력 회복/강화
            if (stage.GrantsSkill)
            {
                learned = SkillBook.LearnRandom(_skillPool, _random);
            }

            if (stage.ClearReward != null)
            {
                rewards.AddRange(RewardApplier.Apply(stage.ClearReward, Player));
            }

            _pendingResult = new StageClearResult(CurrentStageInfo(), rewards, learned);
            _machine.ChangeState(_resultState);
        }

        /// <summary>
        /// 다음 단계로 넘어갑니다. 챕터의 마지막이면 다음 챕터로, 마지막 챕터면 승리로 끝납니다.
        /// </summary>
        internal void AdvanceToNextStage()
        {
            if (StageIndex + 1 < CurrentChapter.Stages.Count)
            {
                StageIndex++;
                BeginStage();
                return;
            }

            if (ChapterIndex + 1 < _chapters.Count)
            {
                ChapterIndex++;
                StageIndex = 0;
                PrepareChapter();
                BeginStage();
                return;
            }

            _machine.ChangeState(_victoryState);
        }

        private StageInfo CurrentStageInfo()
        {
            return new StageInfo(ChapterIndex, StageIndex, CurrentChapter, CurrentStage);
        }
    }
}