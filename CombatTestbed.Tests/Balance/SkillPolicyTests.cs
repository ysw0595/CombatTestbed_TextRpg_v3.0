using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Balance;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Common;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Tests.Balance
{
    [TestFixture]
    public class SkillPolicyTests
    {
        private static CampaignManager CreateCampaign(int maxHp = 100, int attack = 100)
        {
            var hero = new ActorDefinition("hero", "용사", ActorRole.Player, maxHp, attack);
            return DefaultCampaign.Create(new SystemRandomSource(1), hero);
        }

        [Test]
        public void 첫_스킬_전략은_항상_시작_스킬을_골라야_한다()
        {
            CampaignManager campaign = CreateCampaign();
            campaign.SkillBook.Learn(DefaultSkills.Thirster);

            var picked = new FirstSkillPolicy().Choose(campaign, new SystemRandomSource(1));

            Assert.That(picked.Id, Is.EqualTo(DefaultSkills.BatFire.Id));
        }

        [Test]
        public void 최근_스킬_전략은_가장_나중에_배운_스킬을_골라야_한다()
        {
            CampaignManager campaign = CreateCampaign();
            campaign.SkillBook.Learn(DefaultSkills.Thirster);
            campaign.SkillBook.Learn(DefaultSkills.OverFlow);

            var picked = new NewestSkillPolicy().Choose(campaign, new SystemRandomSource(1));

            Assert.That(picked.Id, Is.EqualTo(DefaultSkills.OverFlow.Id));
        }

        [Test]
        public void 무작위_전략은_항상_배운_스킬_안에서_여러_종류를_골라야_한다()
        {
            CampaignManager campaign = CreateCampaign();
            campaign.SkillBook.Learn(DefaultSkills.Thirster);
            campaign.SkillBook.Learn(DefaultSkills.OverFlow);
            var policy = new RandomSkillPolicy();
            var random = new SystemRandomSource(7);

            var picked = Enumerable.Range(0, 100).Select(_ => policy.Choose(campaign, random)).ToList();

            Assert.That(picked.All(s => campaign.SkillBook.Contains(s)), Is.True);
            Assert.That(picked.Select(s => s.Id).Distinct().Count(), Is.GreaterThan(1));
        }

        [Test]
        public void 최대_피해_전략은_자해_피해가_작으면_큰_한_방_스킬을_골라야_한다()
        {
            // 최대 체력 10 -> Blood_Explosion의 자해 피해는 3, 상대에게는 공격력 100 x 2.4 = 240
            CampaignManager campaign = CreateCampaign(maxHp: 10, attack: 100);
            campaign.SkillBook.Learn(DefaultSkills.BloodExplosion);

            var picked = new GreedyDamagePolicy().Choose(campaign, new SystemRandomSource(1));

            Assert.That(picked.Id, Is.EqualTo(DefaultSkills.BloodExplosion.Id));
        }

        [Test]
        public void 최대_피해_전략은_자해_피해가_더_크면_기본_공격을_골라야_한다()
        {
            // 최대 체력 1000 -> 자해 피해 300, 순이득 240 - 300 < 기본 공격의 100
            CampaignManager campaign = CreateCampaign(maxHp: 1000, attack: 100);
            campaign.SkillBook.Learn(DefaultSkills.BloodExplosion);

            var picked = new GreedyDamagePolicy().Choose(campaign, new SystemRandomSource(1));

            Assert.That(picked.Id, Is.EqualTo(DefaultSkills.BatFire.Id));
        }
    }
}