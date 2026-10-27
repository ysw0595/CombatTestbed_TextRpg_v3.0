using System;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using CombatTestbed.Presentation.Choices;
using CombatTestbed.Presentation.Screens;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Presentation.Screens
{
    [TestFixture]
    public class StageResultPresenterTests
    {
        private static StageResultPresenter Create(out FakeStageResultView view, out FakeChoiceView choiceView)
        {
            view = new FakeStageResultView();
            choiceView = new FakeChoiceView();
            return new StageResultPresenter(view, new ChoicePresenter(choiceView));
        }

        private static RewardDefinition RewardOf(params SkillEffectData[] effects)
        {
            return new RewardDefinition("r", effects);
        }

        [Test]
        public void 몬스터를_처치하면_받은_보상이_문구로_보이고_다음_선택지가_나와야_한다()
        {
            var presenter = Create(out var view, out var choiceView);
            var slime = TestCampaigns.Monster("slime", 30, 1,
                RewardOf(SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.MaxHp, 5)));
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Battle), slime);
            presenter.Bind(campaign);
            campaign.Start();

            campaign.UseSkill(TestCampaigns.Strike);

            Assert.That(view.Title, Is.EqualTo("챕터 1 : c1 (stage 1) 클리어"));
            Assert.That(view.Lines, Is.EqualTo(new[] { "최대 체력이 5 증가했다." }));
            Assert.That(choiceView.Shown[0].Id, Is.EqualTo("continue"));
        }

        [Test]
        public void 에피소드에서_스킬을_배우면_스킬_문구가_먼저_나오고_이어서_보상_문구가_나와야_한다()
        {
            var presenter = Create(out var view, out _);
            var skillB = new SkillDefinition("b", "B", new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 2.0f)
            });
            var episode = new StageDefinition(
                StageType.Episode,
                grantsSkill: true,
                clearReward: RewardOf(SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 3)));
            var campaign = TestCampaigns.SingleStage(episode, skillPool: new[] { skillB });
            presenter.Bind(campaign);
            campaign.Start();

            campaign.Continue(); // 에피소드를 마침

            Assert.That(view.Lines, Is.EqualTo(new[] { "B이(가) 추가되었습니다.", "공격력이 3 증가했다." }));
        }

        [Test]
        public void 보상이_없는_단계는_빈_문구로_보이고_다음을_선택하면_진행해야_한다()
        {
            var presenter = Create(out var view, out _);
            var campaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(campaign);
            campaign.Start();
            campaign.Continue(); // 프롤로그 -> 단계 결과

            Assert.That(view.Lines.Count, Is.EqualTo(0));

            presenter.HandleChoice(new ActionChoice("other", "x"));
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult)); // 알 수 없는 선택은 무시

            presenter.HandleChoice(new ActionChoice("continue", "다음"));
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Victory)); // 마지막 단계라 승리로 끝남
        }

        [Test]
        public void 보상_결과는_보여_줄_내용이_있을_때만_문구로_바뀌어야_한다()
        {
            var actor = TestActorFactory.Create(name: "용사");

            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.StatGain, actor, 5, StatType.MaxHp)),
                Is.EqualTo("최대 체력이 5 증가했다."));
            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.StatGain, actor, 4, StatType.Attack)),
                Is.EqualTo("공격력이 4 증가했다."));
            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.StatGain, actor, 2, StatType.Defense)),
                Is.EqualTo("방어력이 2 증가했다."));
            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.Heal, actor, 20)),
                Is.EqualTo("체력을 20 회복했다."));

            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.Heal, actor, 0)), Is.Null);
            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.StatGain, actor, 0, StatType.Attack)), Is.Null);
            Assert.That(RewardMessages.Format(new EffectOutcome(SkillEffectKind.Damage, actor, 10)), Is.Null);
        }

        [Test]
        public void 연결_교체와_Dispose와_잘못된_입력을_올바르게_처리해야_한다()
        {
            var presenter = Create(out var view, out _);
            var oldCampaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            var newCampaign = TestCampaigns.SingleStage(new StageDefinition(StageType.Prologue));
            presenter.Bind(oldCampaign);
            presenter.Bind(newCampaign);

            oldCampaign.Start();
            oldCampaign.Continue();
            Assert.That(view.ShowCount, Is.EqualTo(0)); // 연결이 해제된 캠페인

            newCampaign.Start();
            newCampaign.Continue();
            Assert.That(view.ShowCount, Is.EqualTo(1));

            presenter.Dispose();
            presenter.Dispose();
            Assert.Throws<ObjectDisposedException>(() => presenter.Bind(newCampaign));
            Assert.Throws<ArgumentNullException>(() => Create(out _, out _).Bind(null));
        }
    }
}