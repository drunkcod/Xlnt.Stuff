using System.Collections.Generic;
using NUnit.Framework;

namespace Xlnt.Stuff.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Test]
        public void Each_should_enumerate_all_values()
        {
            var items = new[] {1, 2, 3};
            var target = new List<int>();

            items.ForEach(target.Add);

            Assert.That(target, Is.EqualTo(items));
        }
    }
}
