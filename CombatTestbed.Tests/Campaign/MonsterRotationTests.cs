using System;
using NUnit.Framework;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Tests.TestUtils;

namespace CombatTestbed.Tests.Campaign
{
    [TestFixture]
    public class MonsterRotationTests
    {
        private static ActorDefinition Monster(string id)
        {
            return new ActorDefinition(id, id, ActorRole.Enemy, 10, 1);
        }

        [Test]
        public void 난수가_정한_위치에서_시작해_풀을_순환해야_한다()
        {
            var random = new SequenceRandomSource(1);
            var rotation = new MonsterRotation(new[] { Monster("a"), Monster("b"), Monster("c") }, random);

            var ids = new[]
            {
                rotation.Next().Id, rotation.Next().Id, rotation.Next().Id, rotation.Next().Id
            };

            Assert.That(ids, Is.EqualTo(new[] { "b", "c", "a", "b" }));
            Assert.That(random.CallCount, Is.EqualTo(1));       // 난수는 시작 위치를 정할 때 한 번만
            Assert.That(random.LastMaxExclusive, Is.EqualTo(3)); // 범위는 [0, 풀 크기)
        }

        [Test]
        public void 풀에_몬스터가_하나뿐이면_계속_같은_몬스터를_반환해야_한다()
        {
            var rotation = new MonsterRotation(new[] { Monster("a") }, new SequenceRandomSource(0));

            Assert.That(rotation.Next().Id, Is.EqualTo("a"));
            Assert.That(rotation.Next().Id, Is.EqualTo("a"));
            Assert.That(rotation.Next().Id, Is.EqualTo("a"));
        }

        [Test]
        public void 잘못된_입력으로는_순환기를_만들_수_없어야_한다()
        {
            var random = new SequenceRandomSource(0);

            Assert.Throws<ArgumentNullException>(() => new MonsterRotation(null, random));
            Assert.Throws<ArgumentNullException>(() => new MonsterRotation(new[] { Monster("a") }, null));
            Assert.Throws<ArgumentException>(() => new MonsterRotation(new ActorDefinition[0], random));
            Assert.Throws<ArgumentException>(() => new MonsterRotation(new ActorDefinition[] { null }, random));
        }
    }
}