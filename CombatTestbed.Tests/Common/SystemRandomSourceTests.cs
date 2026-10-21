using System;
using System.Collections.Generic;
using NUnit.Framework;
using CombatTestbed.Core.Common;

namespace CombatTestbed.Tests.Common
{
    [TestFixture]
    public class SystemRandomSourceTests
    {
        [Test]
        public void 지정한_범위_안의_값만_반환하고_범위의_모든_값이_나와야_한다()
        {
            var random = new SystemRandomSource(12345);
            var seen = new HashSet<int>();

            for (int i = 0; i < 1000; i++)
            {
                int value = random.Next(3, 7);
                Assert.That(value, Is.InRange(3, 6));
                seen.Add(value);
            }

            Assert.That(seen, Is.EquivalentTo(new[] { 3, 4, 5, 6 }));
        }

        [Test]
        public void 같은_시드는_같은_순서의_난수를_만들어야_한다()
        {
            var first = new SystemRandomSource(777);
            var second = new SystemRandomSource(777);

            for (int i = 0; i < 10; i++)
            {
                Assert.That(first.Next(0, 100), Is.EqualTo(second.Next(0, 100)));
            }
        }

        [Test]
        public void 범위가_잘못되면_예외가_발생하고_길이_1인_범위는_허용되어야_한다()
        {
            var random = new SystemRandomSource(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(5, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(5, 3));
            Assert.That(random.Next(0, 1), Is.EqualTo(0));
        }
    }
}