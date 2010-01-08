using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xlnt.NUnit
{
    class Scenario : IEnumerable<TestCaseData>
    {
        readonly List<TestCaseData> tests = new List<TestCaseData>();
        string stimuli;
        Action stimulate;

        public Scenario Given(string context, Action establishContext) {
            AddTest("Given " + context, establishContext);
            return this;
        }

        public Scenario When(string stimuli, Action stimulate) {
            this.stimuli = " When " + stimuli;
            this.stimulate = stimulate;
            return this;
        }

        public Scenario Then(string happens, Action check) {
            var thisStimuli = stimulate;
            AddTest(stimuli + " Then " + happens, () => { thisStimuli(); check(); });
            return this;
        }

        public Scenario And(string somethingMore, Action check) {
            AddTest("    And " + somethingMore, check);
            return this;
        }

        void AddTest(string name, Action action) {
            tests.Add(new TestCaseData(action).SetName(name));
        }

        IEnumerator<TestCaseData> IEnumerable<TestCaseData>.GetEnumerator() { return tests.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return tests.GetEnumerator(); }
    }
}
