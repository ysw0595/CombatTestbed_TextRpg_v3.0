using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Skills;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Skills
{
    [TestFixture]
    public class SkillBookTests
    {
        private static SkillDefinition Skill(string id)
        {
            return new SkillDefinition(id, id, new[]
            {
                SkillEffectData.Damage(EffectTarget.Opponent, StatType.Attack, 1.0f)
            });
        }

        [Test]
        public void 새_스킬을_배우면_목록에_추가되고_이벤트가_발행되며_같은_Id는_중복으로_배울_수_없어야_한다()
        {
            var book = new SkillBook();
            var learned = new List<string>();
            book.SkillLearned += skill => learned.Add(skill.Id);

            bool first = book.Learn(Skill("a"));
            bool duplicate = book.Learn(Skill("a")); // 같은 Id, 다른 인스턴스

            Assert.That(first, Is.True);
            Assert.That(duplicate, Is.False);
            Assert.That(book.Skills.Count, Is.EqualTo(1));
            Assert.That(learned, Is.EqualTo(new[] { "a" }));
            Assert.That(book.Contains(Skill("a")), Is.True);
        }

        [Test]
        public void 무작위로_배울_때는_아직_배우지_않은_스킬_중에서만_골라_배워야_한다()
        {
            var book = new SkillBook();
            book.Learn(Skill("a"));
            var pool = new[] { Skill("a"), Skill("b"), Skill("c") };
            var random = new SequenceRandomSource(1); // 후보 [b, c] 중 1번째 = c

            SkillDefinition picked = book.LearnRandom(pool, random);

            Assert.That(picked.Id, Is.EqualTo("c"));
            Assert.That(book.Skills.Count, Is.EqualTo(2));
            Assert.That(random.LastMinInclusive, Is.EqualTo(0));
            Assert.That(random.LastMaxExclusive, Is.EqualTo(2)); // 이미 배운 a는 후보에서 제외
        }

        [Test]
        public void 배울_스킬이_남아있지_않으면_null을_반환하고_난수를_쓰지_않아야_한다()
        {
            var book = new SkillBook();
            book.Learn(Skill("a"));
            var random = new SequenceRandomSource();

            SkillDefinition picked = book.LearnRandom(new[] { Skill("a") }, random);

            Assert.That(picked, Is.Null);
            Assert.That(random.CallCount, Is.EqualTo(0));
        }
    }
}