using System.Collections.Generic;
using NUnit.Framework;
using Xlnt.NUnit;

namespace Xlnt.Stuff.Tests
{
    [TestFixture]
    public class IEnumerableExtensionsTests : ScenarioFixture
    {
        public Scenario Extensions() {
            return new Scenario()
                .Given("a sample sequence [1,2,3]", () => new[] { 1, 2, 3 })
                .Then("ForEach enumerates all values", items => {
                    var target = new List<int>();
                    items.ForEach(target.Add);
                    Assert.That(target, Is.EqualTo(items));
                });
        }
    }
}
