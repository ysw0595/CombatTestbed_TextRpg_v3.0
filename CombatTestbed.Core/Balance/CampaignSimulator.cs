#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Domain.Stats;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 캠페인을 사람 없이 끝까지 자동으로 진행해 결과를 모으는 시뮬레이터입니다.
    /// 캠페인을 "난수 공급자를 받아 새로 만드는 함수"로 받아서, 코드 기본값이든 SO 데이터든 똑같이 돌릴 수 있습니다.
    /// </summary>
    public sealed class CampaignSimulator
    {
        /// <summary>한 판에서 허용하는 스킬 사용 횟수의 기본 상한입니다.</summary>
        public const int DefaultMaxActionsPerGame = 10000;

        private readonly Func<IRandomSource, CampaignManager> _campaignFactory;
        private readonly ISkillPolicy _policy;
        private readonly int _maxActionsPerGame;

        /// <param name="campaignFactory">난수 공급자를 받아 새 캠페인을 만드는 함수 (매 판마다 호출됨)</param>
        /// <param name="policy">전투에서 스킬을 고르는 전략</param>
        /// <param name="maxActionsPerGame">한 판의 스킬 사용 횟수 상한. 넘으면 Stalled로 기록하고 그 판을 끝냅니다.</param>
        public CampaignSimulator(
            Func<IRandomSource, CampaignManager> campaignFactory,
            ISkillPolicy policy,
            int maxActionsPerGame = DefaultMaxActionsPerGame)
        {
            _campaignFactory = campaignFactory ?? throw new ArgumentNullException(nameof(campaignFactory));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));

            if (maxActionsPerGame <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxActionsPerGame), "행동 상한은 1 이상이어야 합니다.");
            }

            _maxActionsPerGame = maxActionsPerGame;
        }

        /// <param name="games">진행할 판 수</param>
        /// <param name="seed">같은 시드는 같은 결과를 만듭니다. (재현과 비교용)</param>
        public SimulationReport Run(int games, int seed)
        {
            if (games <= 0) throw new ArgumentOutOfRangeException(nameof(games), "판 수는 1 이상이어야 합니다.");

            // 시드 하나에서 판마다의 시드를 뽑아, 판들이 서로 독립이면서도 전체는 재현되게 합니다.
            var master = new SystemRandomSource(seed);
            var records = new List<GameRecord>(games);

            for (int i = 0; i < games; i++)
            {
                var campaignRandom = new SystemRandomSource(master.Next(0, int.MaxValue));
                var policyRandom = new SystemRandomSource(master.Next(0, int.MaxValue));

                CampaignManager campaign = _campaignFactory(campaignRandom)
                    ?? throw new InvalidOperationException("캠페인 팩토리가 null을 반환했습니다.");

                records.Add(PlayOne(campaign, policyRandom));
            }

            return new SimulationReport(_policy.Name, seed, records);
        }

        private GameRecord PlayOne(CampaignManager campaign, IRandomSource policyRandom)
        {
            campaign.Start();

            int actions = 0;
            int stagesCleared = 0;

            while (campaign.Phase != CampaignPhase.Victory && campaign.Phase != CampaignPhase.Defeat)
            {
                if (actions >= _maxActionsPerGame)
                {
                    return CreateRecord(campaign, GameOutcome.Stalled, stagesCleared, actions);
                }

                switch (campaign.Phase)
                {
                    case CampaignPhase.Story:
                        campaign.Continue();
                        break;

                    case CampaignPhase.StageResult:
                        stagesCleared++;
                        campaign.Continue();
                        break;

                    case CampaignPhase.Battle:
                        actions++;
                        campaign.UseSkill(_policy.Choose(campaign, policyRandom));
                        break;

                    default:
                        throw new InvalidOperationException($"예상하지 못한 단계입니다: {campaign.Phase}");
                }
            }

            GameOutcome outcome = campaign.Phase == CampaignPhase.Victory ? GameOutcome.Victory : GameOutcome.Defeat;
            return CreateRecord(campaign, outcome, stagesCleared, actions);
        }

        private static GameRecord CreateRecord(CampaignManager campaign, GameOutcome outcome, int stagesCleared, int actions)
        {
            return new GameRecord(
                outcome,
                campaign.ChapterIndex,
                campaign.StageIndex,
                outcome == GameOutcome.Victory ? null : campaign.CurrentEnemy?.Id,
                stagesCleared,
                actions,
                campaign.Player.Health.MaxHp,
                campaign.Player.Stats.GetValue(StatType.Attack));
        }
    }
}