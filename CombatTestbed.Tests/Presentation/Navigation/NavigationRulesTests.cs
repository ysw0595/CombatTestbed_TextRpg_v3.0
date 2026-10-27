using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Presentation.Navigation;

namespace CombatTestbed.Tests.Presentation.Navigation
{
    [TestFixture]
    public class NavigationRulesTests
    {
        [Test]
        public void 허용된_화면_이동은_표에_있는_10가지뿐이어야_한다()
        {
            var allowed = new HashSet<(ScreenId, ScreenId)>
            {
                (ScreenId.Title, ScreenId.Story),
                (ScreenId.Title, ScreenId.Battle),
                (ScreenId.Story, ScreenId.StageResult),
                (ScreenId.Battle, ScreenId.StageResult),
                (ScreenId.Battle, ScreenId.Defeat),
                (ScreenId.StageResult, ScreenId.Story),
                (ScreenId.StageResult, ScreenId.Battle),
                (ScreenId.StageResult, ScreenId.Victory),
                (ScreenId.Victory, ScreenId.Title),
                (ScreenId.Defeat, ScreenId.Title)
            };

            // 6 x 6 = 36가지 조합을 모두 확인: 표에 있는 것만 허용되어야 함
            foreach (ScreenId from in (ScreenId[])Enum.GetValues(typeof(ScreenId)))
            {
                foreach (ScreenId to in (ScreenId[])Enum.GetValues(typeof(ScreenId)))
                {
                    Assert.That(NavigationRules.CanNavigate(from, to), Is.EqualTo(allowed.Contains((from, to))),
                        $"{from} -> {to}");
                }
            }
        }

        [Test]
        public void 아직_화면이_없을_때는_시작_화면으로만_이동할_수_있어야_한다()
        {
            foreach (ScreenId to in (ScreenId[])Enum.GetValues(typeof(ScreenId)))
            {
                Assert.That(NavigationRules.CanNavigate(null, to), Is.EqualTo(to == ScreenId.Title), $"(없음) -> {to}");
            }
        }

        [Test]
        public void 캠페인_단계는_대응하는_화면으로_매핑되어야_한다()
        {
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.NotStarted), Is.EqualTo(ScreenId.Title));
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.Story), Is.EqualTo(ScreenId.Story));
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.Battle), Is.EqualTo(ScreenId.Battle));
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.StageResult), Is.EqualTo(ScreenId.StageResult));
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.Victory), Is.EqualTo(ScreenId.Victory));
            Assert.That(CampaignScreenMap.FromPhase(CampaignPhase.Defeat), Is.EqualTo(ScreenId.Defeat));
            Assert.Throws<ArgumentOutOfRangeException>(() => CampaignScreenMap.FromPhase((CampaignPhase)99));
        }
    }
}