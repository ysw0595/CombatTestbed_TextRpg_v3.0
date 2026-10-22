using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Combat;
using CombatTestbed.Core.Content;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Campaign
{
    [TestFixture]
    public class CampaignManagerTests
    {
        // 공격력 50인 용사가 쓰는 기본 스킬: 50 피해
        private static readonly SkillDefinition Strike = DamageSkill("strike", 1.0f);

        private static SkillDefinition DamageSkill(string id, float coefficient)
        {
            return new SkillDefinition(id, id, new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, coefficient)
            });
        }

        private static ActorDefinition Hero(int maxHp = 100, int attack = 50)
        {
            return new ActorDefinition("hero", "용사", ActorRole.Player, maxHp, attack);
        }

        private static ActorDefinition Monster(
            string id, int maxHp, int attack, RewardDefinition reward = null, SkillDefinition[] skills = null)
        {
            return new ActorDefinition(id, id, ActorRole.Enemy, maxHp, attack, reward: reward, behaviorSkills: skills);
        }

        private static StageDefinition Stage(
            StageType type, bool grantsSkill = false, RewardDefinition clearReward = null)
        {
            return new StageDefinition(type, grantsSkill, clearReward);
        }

        private static ChapterDefinition Chapter(
            string id, StageDefinition[] stages, ActorDefinition[] pool = null, ActorDefinition boss = null)
        {
            return new ChapterDefinition(id, id, stages, pool, boss);
        }

        private static CampaignManager CreateCampaign(
            ChapterDefinition[] chapters, SequenceRandomSource random, SkillDefinition[] skillPool = null)
        {
            return new CampaignManager(
                chapters, Hero(), new[] { Strike }, skillPool ?? new SkillDefinition[0], random);
        }

        // 보스 한 명(체력 80, 반격 20)을 상대하고 클리어하면 체력을 회복하는 캠페인
        private static CampaignManager CreateSingleBossCampaign()
        {
            var boss = Monster("boss", 80, 10, skills: new[] { DamageSkill("a", 2f) });
            var chapter = Chapter(
                "c1",
                new[] { Stage(StageType.Boss, clearReward: DefaultRewards.MoonBlessing) },
                boss: boss);

            return CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));
        }

        [Test]
        public void 시작_전에는_NotStarted이고_Start하면_첫_단계의_스토리_단계가_되어야_한다()
        {
            var campaign = CreateCampaign(
                new[] { Chapter("c1", new[] { Stage(StageType.Prologue) }) }, new SequenceRandomSource());
            var phases = new List<string>();
            campaign.PhaseChanged += (previous, next) => phases.Add($"{previous}->{next}");
            StageInfo? started = null;
            campaign.StageStarted += info => started = info;
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.NotStarted));

            campaign.Start();

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Story));
            Assert.That(phases, Is.EqualTo(new[] { "NotStarted->Story" }));
            Assert.That(started.HasValue, Is.True);
            Assert.That(started.Value.ChapterIndex, Is.EqualTo(0));
            Assert.That(started.Value.StageIndex, Is.EqualTo(0));
            Assert.That(started.Value.Stage.Type, Is.EqualTo(StageType.Prologue));
            Assert.Throws<InvalidOperationException>(() => campaign.Start());
        }

        [Test]
        public void 스토리_단계를_마치면_단계_결과를_거쳐_다음_단계인_전투가_시작되어야_한다()
        {
            var chapter = Chapter(
                "c1",
                new[] { Stage(StageType.Prologue), Stage(StageType.Battle) },
                new[] { Monster("slime", 30, 1) });
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));
            StageClearResult cleared = null;
            TurnManager battle = null;
            campaign.StageCleared += result => cleared = result;
            campaign.BattleStarted += b => battle = b;
            campaign.Start();

            campaign.Continue(); // 프롤로그 -> 단계 결과

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
            Assert.That(cleared, Is.Not.Null);
            Assert.That(cleared.Stage.StageIndex, Is.EqualTo(0));
            Assert.That(cleared.Rewards.Count, Is.EqualTo(0));
            Assert.That(cleared.LearnedSkill, Is.Null);

            campaign.Continue(); // 단계 결과 -> 전투

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Battle));
            Assert.That(battle, Is.Not.Null);
            Assert.That(campaign.CurrentEnemy.Id, Is.EqualTo("slime"));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.PlayerTurn));
        }

        [Test]
        public void 전투에서_승리하면_몬스터_보상을_받고_전투_중_상태이상이_정리되어야_한다()
        {
            var slime = Monster("slime", 30, 1, reward: RewardOf(SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 5)));
            var chapter = Chapter("c1", new[] { Stage(StageType.Battle) }, new[] { slime });
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));
            StageClearResult cleared = null;
            campaign.StageCleared += result => cleared = result;
            // 전투가 시작될 때 용사에게 출혈이 걸린 상황
            campaign.BattleStarted += b =>
                campaign.Player.StatusEffects.ApplyEffect(campaign.Player, new BleedEffect(1));
            campaign.Start();
            Assert.That(campaign.Player.StatusEffects.HasEffect(StatusEffectType.Bleed), Is.True);

            campaign.UseSkill(Strike); // 50 피해로 슬라임(30) 처치

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
            Assert.That(cleared.Rewards.Count, Is.EqualTo(1));
            Assert.That(cleared.Rewards[0].Kind, Is.EqualTo(SkillEffectKind.StatGain));
            Assert.That(cleared.Rewards[0].Stat, Is.EqualTo(StatType.Attack));
            Assert.That(cleared.Rewards[0].Amount, Is.EqualTo(5));
            Assert.That(campaign.Player.Stats.GetValue(StatType.Attack), Is.EqualTo(55));
            Assert.That(campaign.Player.StatusEffects.HasEffect(StatusEffectType.Bleed), Is.False);
        }

        [Test]
        public void 에피소드를_마치면_아직_배우지_않은_스킬_중_하나를_배우고_결과에_담겨야_한다()
        {
            var chapter = Chapter("c1", new[] { Stage(StageType.Episode, grantsSkill: true) });
            var pool = new[] { DamageSkill("skillB", 2f), DamageSkill("skillC", 3f) };
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(1), pool); // 후보 [B, C] 중 1번째 = C
            StageClearResult cleared = null;
            var learned = new List<string>();
            campaign.StageCleared += result => cleared = result;
            campaign.SkillBook.SkillLearned += skill => learned.Add(skill.Id);
            campaign.Start();

            campaign.Continue();

            Assert.That(cleared.LearnedSkill.Id, Is.EqualTo("skillC"));
            Assert.That(campaign.SkillBook.Skills.Select(s => s.Id), Is.EqualTo(new[] { "strike", "skillC" }));
            Assert.That(learned, Is.EqualTo(new[] { "skillC" }));
        }

        [Test]
        public void 보스는_난수로_정한_스킬부터_순환하며_시작_스킬은_마지막_스킬을_제외한_범위에서_골라야_한다()
        {
            var boss = Monster("boss", 500, 10, skills: new[]
            {
                DamageSkill("a", 1f), DamageSkill("b", 2f), DamageSkill("c", 3f)
            });
            var random = new SequenceRandomSource(1); // 시작 스킬 = b
            var campaign = CreateCampaign(new[] { Chapter("c1", new[] { Stage(StageType.Boss) }, boss: boss) }, random);
            campaign.Start();

            Assert.That(random.LastMaxExclusive, Is.EqualTo(2)); // 스킬 3개 -> 앞 2개 중에서 선택

            for (int i = 0; i < 4; i++)
            {
                campaign.UseSkill(Strike); // 보스(체력 500)는 살아 있음
            }

            // 보스 반격: b(20) -> c(30) -> a(10) -> b(20)
            Assert.That(campaign.Player.Health.CurrentHp, Is.EqualTo(100 - (20 + 30 + 10 + 20)));
        }

        [Test]
        public void 보스를_처치하면_클리어_보상으로_체력을_회복하고_결과에_실제_회복량이_담겨야_한다()
        {
            var campaign = CreateSingleBossCampaign();
            StageClearResult cleared = null;
            campaign.StageCleared += result => cleared = result;
            campaign.Start();

            campaign.UseSkill(Strike); // 보스 80 -> 30, 보스 반격 20 -> 용사 80
            Assert.That(campaign.Player.Health.CurrentHp, Is.EqualTo(80));
            campaign.UseSkill(Strike); // 보스 처치

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.StageResult));
            Assert.That(cleared.Rewards.Count, Is.EqualTo(1));
            Assert.That(cleared.Rewards[0].Kind, Is.EqualTo(SkillEffectKind.Heal));
            Assert.That(cleared.Rewards[0].Amount, Is.EqualTo(20));
            Assert.That(campaign.Player.Health.CurrentHp, Is.EqualTo(100));
        }

        [Test]
        public void 마지막_단계의_결과에서_다음으로_넘어가면_승리로_끝나야_한다()
        {
            var campaign = CreateSingleBossCampaign();
            var results = new List<CampaignResult>();
            campaign.CampaignEnded += result => results.Add(result);
            campaign.Start();
            campaign.UseSkill(Strike);
            campaign.UseSkill(Strike);

            campaign.Continue(); // 단계 결과 -> 마지막이므로 승리

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Victory));
            Assert.That(results, Is.EqualTo(new[] { CampaignResult.Victory }));
            Assert.Throws<InvalidOperationException>(() => campaign.Continue());
        }

        [Test]
        public void 플레이어가_사망하면_패배로_끝나고_이후_입력은_거부되어야_한다()
        {
            var brute = Monster("brute", 1000, 100); // 용사(체력 100)를 한 방에 쓰러뜨림
            var chapter = Chapter("c1", new[] { Stage(StageType.Battle) }, new[] { brute });
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));
            var results = new List<CampaignResult>();
            campaign.CampaignEnded += result => results.Add(result);
            campaign.Start();

            campaign.UseSkill(Strike); // 브루트 생존 -> 반격으로 용사 사망

            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Defeat));
            Assert.That(results, Is.EqualTo(new[] { CampaignResult.Defeat }));
            Assert.Throws<InvalidOperationException>(() => campaign.UseSkill(Strike));
            Assert.Throws<InvalidOperationException>(() => campaign.Continue());
        }

        [Test]
        public void 허용되지_않는_단계의_입력과_배우지_않은_스킬은_거부되어야_한다()
        {
            var chapter = Chapter(
                "c1",
                new[] { Stage(StageType.Prologue), Stage(StageType.Battle) },
                new[] { Monster("slime", 30, 1) });
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));

            // 시작 전
            Assert.Throws<InvalidOperationException>(() => campaign.Continue());
            Assert.Throws<InvalidOperationException>(() => campaign.UseSkill(Strike));

            campaign.Start();

            // 스토리 단계
            Assert.Throws<InvalidOperationException>(() => campaign.UseSkill(Strike));
            Assert.Throws<ArgumentNullException>(() => campaign.UseSkill(null));

            campaign.Continue(); // 단계 결과

            Assert.Throws<InvalidOperationException>(() => campaign.UseSkill(Strike));

            campaign.Continue(); // 전투

            Assert.Throws<InvalidOperationException>(() => campaign.Continue());
            Assert.Throws<ArgumentException>(() => campaign.UseSkill(DamageSkill("unknown", 1f)));
        }

        [Test]
        public void 챕터의_마지막_단계를_마치면_다음_챕터의_첫_단계로_넘어가고_새_순환기를_준비해야_한다()
        {
            var random = new SequenceRandomSource(0);
            var chapter1 = Chapter("c1", new[] { Stage(StageType.Prologue) });
            var chapter2 = Chapter(
                "c2",
                new[] { Stage(StageType.Prologue), Stage(StageType.Battle) },
                new[] { Monster("slime", 30, 1) });
            var campaign = CreateCampaign(new[] { chapter1, chapter2 }, random);
            var started = new List<string>();
            campaign.StageStarted += info => started.Add($"{info.Chapter.Id}:{info.StageIndex}");

            campaign.Start();
            Assert.That(random.CallCount, Is.EqualTo(0)); // 챕터 1에는 몬스터 풀이 없어 난수를 쓰지 않음

            campaign.Continue(); // 챕터 1 프롤로그 -> 단계 결과
            campaign.Continue(); // 단계 결과 -> 챕터 2

            Assert.That(started, Is.EqualTo(new[] { "c1:0", "c2:0" }));
            Assert.That(campaign.ChapterIndex, Is.EqualTo(1));
            Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Story));
            Assert.That(random.CallCount, Is.EqualTo(1)); // 챕터 2 시작 시 순환기 시작 위치를 정함
        }

        [Test]
        public void 일반_전투는_챕터의_몬스터_풀을_순서대로_순환해서_등장해야_한다()
        {
            var chapter = Chapter(
                "c1",
                new[] { Stage(StageType.Battle), Stage(StageType.Battle) },
                new[] { Monster("m1", 30, 1), Monster("m2", 30, 1) });
            var campaign = CreateCampaign(new[] { chapter }, new SequenceRandomSource(0));
            var names = new List<string>();
            campaign.BattleStarted += battle => names.Add(battle.Monster.Id);
            campaign.Start();

            campaign.UseSkill(Strike); // m1 처치
            campaign.Continue();       // 단계 결과 -> 다음 전투
            campaign.UseSkill(Strike); // m2 처치

            Assert.That(names, Is.EqualTo(new[] { "m1", "m2" }));
        }

        [Test]
        public void 배울_스킬이_더_없으면_스킬_획득_단계도_스킬_없이_진행되고_난수를_쓰지_않아야_한다()
        {
            var chapter = Chapter("c1", new[] { Stage(StageType.Episode, grantsSkill: true) });
            var random = new SequenceRandomSource();
            var campaign = CreateCampaign(new[] { chapter }, random, new[] { Strike }); // 풀의 스킬을 이미 알고 있음
            StageClearResult cleared = null;
            campaign.StageCleared += result => cleared = result;
            campaign.Start();

            campaign.Continue();

            Assert.That(cleared.LearnedSkill, Is.Null);
            Assert.That(random.CallCount, Is.EqualTo(0));
            Assert.That(campaign.SkillBook.Skills.Count, Is.EqualTo(1));
        }

        [Test]
        public void 잘못된_입력으로는_캠페인을_만들_수_없어야_한다()
        {
            var chapters = new[] { Chapter("c1", new[] { Stage(StageType.Prologue) }) };
            var random = new SequenceRandomSource();
            var skills = new[] { Strike };
            var pool = new SkillDefinition[0];

            Assert.Throws<ArgumentNullException>(() => new CampaignManager(null, Hero(), skills, pool, random));
            Assert.Throws<ArgumentException>(
                () => new CampaignManager(new ChapterDefinition[0], Hero(), skills, pool, random));
            Assert.Throws<ArgumentNullException>(() => new CampaignManager(chapters, null, skills, pool, random));
            Assert.Throws<ArgumentNullException>(() => new CampaignManager(chapters, Hero(), null, pool, random));
            Assert.Throws<ArgumentException>(
                () => new CampaignManager(chapters, Hero(), new SkillDefinition[0], pool, random));
            Assert.Throws<ArgumentNullException>(() => new CampaignManager(chapters, Hero(), skills, null, random));
            Assert.Throws<ArgumentNullException>(() => new CampaignManager(chapters, Hero(), skills, pool, null));
        }

        private static RewardDefinition RewardOf(params SkillEffectData[] effects)
        {
            return new RewardDefinition("test_reward", effects);
        }
    }
}