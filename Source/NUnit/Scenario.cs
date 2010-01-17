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
        static readonly Action Nop = () => { };
        readonly List<TestCaseData> tests;
        string stimuli;

        public Scenario() {
            this.tests = new List<TestCaseData>();
        }

        public Scenario(string description): this() {
            AddTest("Scenario: " + description, Nop);
        }

        protected Scenario(Scenario other) { 
            this.tests = other.tests;
        }

        public Scenario Before(string weStart, Action before) {
            return AddTest(Before(weStart), before);
        }

        public Scenario Describe<T>() {
            return AddTest(Describe(typeof(T).Name), Nop);
        }

        public Scenario Given(string context, Action establishContext) {
            AddTest(Given(context), Nop);
            return new FixtureContextScenario(this, establishContext);
        }

        public ScenarioContext<T> Given<T>(string context, Func<T> establishContext) {
            AddTest(Given(context), Nop);
            return new ScenarioContext<T>(this, establishContext);
        }

        public virtual FixtureContextScenario When(string stimuli, Action stimulate) {
            return new FixtureContextScenario(this, Nop).When(stimuli, stimulate);
        }

        public virtual Scenario<object,T> When<T>(string stimuli, Func<T> stimulate) {          
            return new Scenario<object, T>(this, IgnoreContextResult()).When(stimuli, x => stimulate());
        }

        public Scenario It(string should, Action check) {
            return AddTest(It(should), check);
        }

        protected Scenario AddTest(string name, Action action) {
            tests.Add(new TestCaseData(action).SetName(name));
            return this;
        }

        protected virtual Func<object> IgnoreContextResult() { return () => null; }

        protected string Before(string weStart) { return "Before " + weStart; }
        protected string Describe(string description) { return description; }
        protected string Given(string context) { return "Given " + context; }
        protected void SetWhen(string stimuli) { this.stimuli = "   When " + stimuli; }
        protected string Then(string happens) { return stimuli + " Then " + happens; }
        protected string And(string somethingMore) { return "    And " + somethingMore; }
        private string It(string should) { return "- " + should; }

        IEnumerator<TestCaseData> IEnumerable<TestCaseData>.GetEnumerator() { return tests.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return tests.GetEnumerator(); }
    }

    public class FixtureContextScenario : Scenario
    {
        Action establishContext;
        Action stimulate;

        internal FixtureContextScenario(Scenario other, Action establishContext) : base(other) {
            this.establishContext = establishContext;
        }

        public override FixtureContextScenario When(string stimuli, Action stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public FixtureContextScenario Then(string happens, Action check) {
            var thisContext = establishContext;         
            var thisStimuli = stimulate;
            AddTest(Then(happens), () => { thisContext(); thisStimuli(); check(); });
            return this;
        }

        public FixtureContextScenario And(string somethingMore, Action check) {
            AddTest(And(somethingMore), check);
            return this;
        }

        protected override Func<object> IgnoreContextResult() {
            var thisContext = establishContext;
            return () => { thisContext(); return null; }; 
        }
    }

    public class ScenarioContext<TContext> : Scenario
    {
        protected Func<TContext> establishContext;
        Func<TContext> stimulate;
        
        internal ScenarioContext(Scenario other, Func<TContext> establishContext): base(other) {
            this.establishContext = establishContext;
        }

        public Scenario<TContext,TResult> When<TResult>(string stimuli, Func<TContext,TResult> stimulate) {
            return ToScenario<TResult>().When(stimuli, stimulate);
        }

        public Scenario<TContext,TContext> When(string stimuli, Action<TContext> stimulate) {
            var thisContext = PrepareThen();
            return ToScenario<TContext>().When(stimuli, x => { stimulate(thisContext()); return thisContext(); });
        }

        Scenario<TContext, TResult> ToScenario<TResult>() { return new Scenario<TContext, TResult>(this, establishContext); }

        public ScenarioContext<TContext> Then(string happens, Action<TContext> check) {
            var thisContext = PrepareThen();
            AddTest(Then(happens), () => check(thisContext()));
            return this;
        }

        Func<TContext> PrepareThen() {
            TContext value = default(TContext);
            var localContext = establishContext;
            var thisContext = localContext;
            establishContext = () => { thisContext = localContext; return localContext(); };
            return stimulate = () => {
                if(thisContext != null)
                    value = thisContext();
                thisContext = null;
                return value; };
        }

        public ScenarioContext<TContext> And(string somethingMore, Action<TContext> check) {
            var thisStimulate = stimulate;
            AddTest(And(somethingMore), () => check(thisStimulate()));
            return this;
        }
    }

    public class Scenario<TContext,TResult> : ScenarioContext<TContext>
    {
        Func<TContext,TResult> stimulate;

        internal Scenario(Scenario other, Func<TContext> establishContext): base(other, establishContext) {}

        public Scenario<TContext,TResult> When(string stimuli, Func<TContext,TResult> stimulate) {
            SetWhen(stimuli);
            this.stimulate = stimulate;
            return this;
        }

        public Scenario<TContext,TResult> Then(string happens, Action<TResult> check) {
            TResult value = default(TResult);
            var thisContext = establishContext;
            var thisStimulate = stimulate;
            stimulate = x => value;
            return AddTest(Then(happens), check, () => { return value = thisStimulate(thisContext()); });
        }

        public Scenario<TContext,TResult> And(string somethingMore, Action<TResult> check) {
            return AddTest(And(somethingMore), check, () => stimulate(default(TContext)));
        }

        protected override Func<object> IgnoreContextResult() { 
            var thisContext = establishContext;
            return () => { thisContext(); return null; };
        }

        Scenario<TContext,TResult> AddTest(string description, Action<TResult> check, Func<TResult> thisStimuli) {
            AddTest(description, () => check(thisStimuli()));
            return this;
        }
    }
}
