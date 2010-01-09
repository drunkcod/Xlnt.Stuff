using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xlnt.NUnit
{
    class ScenarioBase : IEnumerable<TestCaseData>
    {
        readonly List<TestCaseData> tests;
        string stimuli;

        public ScenarioBase() {
            this.tests = new List<TestCaseData>();
        }

        protected ScenarioBase(ScenarioBase other) { this.tests = other.tests; }     

        protected void AddTest(string name, Action action) {
            tests.Add(new TestCaseData(action).SetName(name));
        }

        protected string Given(string context) { return "Given " + context; }
        protected void SetWhen(string stimuli) { this.stimuli = " When " + stimuli; }
        protected string Then(string happens) { return stimuli + " Then " + happens; }

        IEnumerator<TestCaseData> IEnumerable<TestCaseData>.GetEnumerator() { return tests.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return tests.GetEnumerator(); }
    }

    class Scenario : ScenarioBase
    {
        Action stimulate;

        public Scenario() { }
        internal Scenario(ScenarioBase other): base(other) { }

        public Scenario Given(string context, Action establishContext) {
            AddTest(Given(context), establishContext);
            return this;
        }

        public Scenario When(string stimuli, Action stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public Scenario<T> When<T>(string stimuli, Func<T> stimulate) {
            return new Scenario<T>(this).When(stimuli, stimulate);
        }

        public Scenario Then(string happens, Action check) {
            var thisStimuli = stimulate;
            AddTest(Then(happens), () => { thisStimuli(); check(); });
            return this;
        }

        public Scenario And(string somethingMore, Action check) {
            AddTest("    And " + somethingMore, check);
            return this;
        }
    }

    class Scenario<T> : ScenarioBase
    {
        Func<T> stimulate;

        internal Scenario(ScenarioBase other): base(other) { }

        public Scenario<T> When(string stimuli, Func<T> stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public Scenario Then(string happens, Action<T> check) {
            var thisStimuli = stimulate;
            AddTest(Then(happens), () => { check(thisStimuli()); });
            return new Scenario(this);
        }
    }
}
