using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;

namespace Xlnt.NUnit
{
    public class ScenarioFixture
    {
        [TestCaseSource("AllScenarios")]
        public void Scenarios(Action verify) { verify(); }

        public IEnumerable<TestCaseData> AllScenarios() {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach(var item in methods) {
                if(typeof(Scenario).IsAssignableFrom(item.ReturnType)){
                    var scenario = item.Invoke(this, null) as IEnumerable<TestCaseData>;
                    foreach(var test in scenario)
                        yield return test;
                }
            }
        }
    }

    public class Scenario : IEnumerable<TestCaseData>
    {
        readonly List<TestCaseData> tests;
        string stimuli;

        public Scenario() {
            this.tests = new List<TestCaseData>();
        }

        public Scenario(string description): this() {
            AddTest("Scenario: " + description, () => { });
        }

        protected Scenario(Scenario other) { this.tests = other.tests; }

        public Scenario Given(string context, Action establishContext) {
            AddTest(Given(context), establishContext);
            return new FixtureContextScenario(this);
        }

        public virtual FixtureContextScenario When(string stimuli, Action stimulate) {
            return new FixtureContextScenario(this).When(stimuli, stimulate);
        }

        public virtual Scenario<T> When<T>(string stimuli, Func<T> stimulate) {
            return new Scenario<T>(this).When(stimuli, stimulate);
        }

        public Scenario It(string should, Action check) {
            AddTest(It(should), check);
            return this;
        }

        protected void AddTest(string name, Action action) {
            tests.Add(new TestCaseData(action).SetName(name));
        }

        protected string Given(string context) { return "Given " + context; }
        protected void SetWhen(string stimuli) { this.stimuli = "   When " + stimuli; }
        protected string Then(string happens) { return stimuli + " Then " + happens; }
        protected string And(string somethingMore) { return "    And " + somethingMore; }
        private string It(string should) { return "  It " + should; }

        IEnumerator<TestCaseData> IEnumerable<TestCaseData>.GetEnumerator() { return tests.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return tests.GetEnumerator(); }
    }

    public class FixtureContextScenario : Scenario
    {
        Action stimulate;

        internal FixtureContextScenario(Scenario other) : base(other) { }

        public override FixtureContextScenario When(string stimuli, Action stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public Scenario Then(string happens, Action check) {
            var thisStimuli = stimulate;
            AddTest(Then(happens), () => { thisStimuli(); check(); });
            return this;
        }

        public Scenario And(string somethingMore, Action check) {
            AddTest(And(somethingMore), check);
            return this;
        }
    }

    public class Scenario<T> : Scenario
    {
        Func<T> stimulate;

        internal Scenario(Scenario other): base(other) { }

        public Scenario<T> When(string stimuli, Func<T> stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public Scenario<T> Then(string happens, Action<T> check) {
            var thisStimuli = stimulate;
            AddTest(Then(happens), () => { check(thisStimuli()); });
            return this;
        }

        public Scenario<T> And(string somethingMore, Action check) {
            AddTest(And(somethingMore), check);
            return this;
        }

    }
}
