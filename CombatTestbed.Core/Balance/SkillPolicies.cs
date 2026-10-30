#nullable enable
using System;
using System.Collections.Generic;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Formulas;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 공통 검증(널, 빈 스킬북)을 맡고, 실제로 고르는 부분만 자식에게 맡기는 기반 클래스입니다.
    /// </summary>
    public abstract class SkillPolicyBase : ISkillPolicy
    {
        public abstract string Name { get; }

        public SkillDefinition Choose(CampaignManager campaign, IRandomSource random)
        {
            if (campaign == null) throw new ArgumentNullException(nameof(campaign));
            if (random == null) throw new ArgumentNullException(nameof(random));

            IReadOnlyList<SkillDefinition> skills = campaign.SkillBook.Skills;

            if (skills.Count == 0)
            {
                throw new InvalidOperationException("배운 스킬이 없어서 고를 수 없습니다.");
            }

            return Pick(campaign, skills, random);
        }

        protected abstract SkillDefinition Pick(
            CampaignManager campaign, IReadOnlyList<SkillDefinition> skills, IRandomSource random);
    }

    /// <summary>배운 스킬 중 무작위로 고릅니다. (아무 생각 없이 누르는 플레이어)</summary>
    public sealed class RandomSkillPolicy : SkillPolicyBase
    {
        public override string Name => "무작위 스킬";

        protected override SkillDefinition Pick(
            CampaignManager campaign, IReadOnlyList<SkillDefinition> skills, IRandomSource random)
        {
            return skills[random.Next(0, skills.Count)];
        }
    }

    /// <summary>항상 가장 먼저 배운 스킬(시작 스킬)만 씁니다. (기본 공격만 누르는 플레이어)</summary>
    public sealed class FirstSkillPolicy : SkillPolicyBase
    {
        public override string Name => "항상 첫 스킬";

        protected override SkillDefinition Pick(
            CampaignManager campaign, IReadOnlyList<SkillDefinition> skills, IRandomSource random)
        {
            return skills[0];
        }
    }

    /// <summary>항상 가장 최근에 배운 스킬을 씁니다. (새로 얻은 스킬만 써 보는 플레이어)</summary>
    public sealed class NewestSkillPolicy : SkillPolicyBase
    {
        public override string Name => "가장 최근에 배운 스킬";

        protected override SkillDefinition Pick(
            CampaignManager campaign, IReadOnlyList<SkillDefinition> skills, IRandomSource random)
        {
            return skills[skills.Count - 1];
        }
    }

    /// <summary>
    /// 지금 당장 (상대에게 줄 피해 - 나에게 돌아오는 피해)가 가장 큰 스킬을 고릅니다.
    /// 동점이면 먼저 배운 스킬입니다. (앞날을 보지 않고 눈앞의 최선만 고르는 플레이어)
    /// </summary>
    public sealed class GreedyDamagePolicy : SkillPolicyBase
    {
        public override string Name => "눈앞의 최대 피해";

        protected override SkillDefinition Pick(
            CampaignManager campaign, IReadOnlyList<SkillDefinition> skills, IRandomSource random)
        {
            SkillDefinition best = skills[0];
            int bestScore = Score(campaign.Player, best);

            for (int i = 1; i < skills.Count; i++)
            {
                int score = Score(campaign.Player, skills[i]);

                if (score > bestScore)
                {
                    best = skills[i];
                    bestScore = score;
                }
            }

            return best;
        }

        private static int Score(BattleActor player, SkillDefinition skill)
        {
            int score = 0;

            foreach (SkillEffectData effect in skill.Effects)
            {
                if (effect.Kind != SkillEffectKind.Damage) continue;

                int amount = CombatFormula.CalculateAmount(
                    effect.FlatValue, player.Stats.GetValue(effect.ScalingStat), effect.Coefficient);

                // 상대에게 주는 피해는 이득, 나에게 주는 피해(Blood_Explosion의 자해 등)는 손해
                score += effect.Target == EffectTarget.Opponent ? amount : -amount;
            }

            return score;
        }
    }
}