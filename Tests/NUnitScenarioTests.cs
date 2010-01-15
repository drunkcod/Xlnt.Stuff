using NUnit.Framework;

namespace Xlnt.NUnit
{
    [TestFixture]
    public class NUnitScenarioTests : ScenarioFixture
    {
        int value, count;

        void CreateContext() { value = 1; }
        int GetValue() { return value; }

        public Scenario SampleScenarios() {
            return new Scenario()
            .Given("that I establish some context", CreateContext)
                .When("I modify it", () => { value *= 2; })
                .Then("I can later use it", () => Assert.That(value, Is.EqualTo(2)))
                .When("I use it for another 'When'", () => GetValue())
                .Then("it has been reinitialized", x => Assert.That(x, Is.EqualTo(1)))

            .Before("starting we clear the context", () => count = 0)
                .Given("that I modify context in the 'Given'", () => ++count)
                .When("I run a scenario", x => x)
                .Then("stuff happens", x => { })
                .When("I run one more scenario", x => x)
                .Then("context is only established once per 'When'", x => Assert.That(count, Is.EqualTo(2)))
                .And("additional checks dont re-establish context", x => Assert.That(count, Is.EqualTo(2)))

            .Describe<Scenario>()
                .It("supports spec like tests via 'It'", () => Assert.True(true))

            .Given("that I establish a implicit context", () => new { Value = 42 })
            .When("when I transform it", x => x)
            .Then("it's possible to use anonymous objects", x => Assert.That(x.Value, Is.EqualTo(42)));
        }

        public Scenario UsingImplicitContext() {
            return new Scenario()
            .Given("that I establish a implicit context", () => new {Value = 2})
                .When("I just pass it throuhg", x => x)
                .Then("ignore checking it", x => { })
                .And("I can later decide to use it", x => Assert.That(x.Value, Is.EqualTo(2)))
                
                .When("I transform it", x => new {Answer = 21 * x.Value})
                .Then("I get the transformed value", x => Assert.That(x.Answer, Is.EqualTo(42)))
                .And("I can use it multiple times", x => Assert.That(x.Answer, Is.EqualTo(42)));            
        }
    }
}
