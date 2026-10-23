using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.Content
{
    [TestFixture]
    public class DefaultCampaignTests
    {
        private static bool IsEnded(CampaignManager campaign)
        {
            return campaign.Phase == CampaignPhase.Victory || campaign.Phase == CampaignPhase.Defeat;
        }

        [Test]
        public void 압도적으로_강한_플레이어가_기본_캠페인을_끝까지_진행하면_30단계를_거쳐_승리하고_스킬_5종을_모두_배워야_한다()
        {
            // 밸런스와 무관하게 흐름만 검증하려고 모든 적을 한 방에 쓰러뜨리는 플레이어를 사용
            var strongHero = new ActorDefinition("hero", "용사", ActorRole.Player, maxHp: 1000, attack: 100000);
            var campaign = DefaultCampaign.Create(new SystemRandomSource(2024), strongHero);
            var started = new List<string>();
            campaign.StageStarted += info => started.Add($"{info.ChapterIndex}-{info.StageIndex}");
            campaign.Start();

            int guard = 0;
            while (!IsEnded(campaign))
            {
                Assert.That(++guard, Is.LessThan(500), "캠페인이 끝나지 않습니다.");

                if (campaign.Phase == CampaignPhase.Battle)
                {
                    campaign.UseSkill(campaign.SkillBook.Skills[0]); // 시작 스킬 Bat_Fire
                }
                else
                {
                    campaign.Continue();
                }
            }

            var expectedStages = Enumerable.Range(0, 3)
                .SelectMany(chapter => Enumerable.Range(0, 10).Select(stage => $"{chapter}-{stage}"));

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Victory));
            Assert.That(started, Is.EqualTo(expectedStages.ToArray()));
            // 시작 1종 + 챕터 1, 2의 에피소드와 보스에서 4종 = 5종 (챕터 3은 스킬 획득 없음)
            Assert.That(campaign.SkillBook.Skills.Count, Is.EqualTo(5));
        }

        [Test]
        public void 기본_흡혈귀가_배운_스킬을_돌려_쓰며_여러_시드로_진행해도_항상_승리나_패배로_끝나야_한다()
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                var campaign = DefaultCampaign.Create(new SystemRandomSource(seed));
                campaign.Start();

                int step = 0;
                while (!IsEnded(campaign))
                {
                    Assert.That(++step, Is.LessThan(5000), $"시드 {seed}: 캠페인이 끝나지 않습니다.");

                    if (campaign.Phase == CampaignPhase.Battle)
                    {
                        var skills = campaign.SkillBook.Skills;
                        campaign.UseSkill(skills[step % skills.Count]);
                    }
                    else
                    {
                        campaign.Continue();
                    }
                }
            }
        }
    }
}