using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Xlnt.Data;

namespace ConsoleApplication23
{
    class TableParamaters : IProfilingSessionQueryListener
    {
        interface IParameterInjector
        {
            void Inject(IDbCommand command);
        }

        class ParameterInjector<T,TColumn> : IParameterInjector
        {
            readonly XmlParameter parameter;
            readonly Expression<Func<T,TColumn>> selector;

            public ParameterInjector(XmlParameter parameter, Expression<Func<T,TColumn>> selector) {
                this.parameter = parameter;
                this.selector = selector;
            }

            public void Inject(IDbCommand command) { parameter.Inject(selector, command); }
        }

        class ParameterScope : IDisposable
        {
            readonly TableParamaters parent;

            public ParameterScope(TableParamaters parent) {
                this.parent = parent;
            }

            void IDisposable.Dispose() {
                parent.parameters.Pop();
            }
        }

        int nextId = 0;
        readonly Stack<IParameterInjector> parameters = new Stack<IParameterInjector>();

        public IDisposable Add<T,TColumn>(IEnumerable<T> values, Expression<Func<T,TColumn>> selector) {
            var newParameter = new XmlParameter("@_" + nextId++);
            var selectorFun = selector.Compile();
            newParameter.AddRange(values.Select(selector.Compile()).Cast<object>());
            parameters.Push(new ParameterInjector<T,TColumn>(newParameter, selector));
            return new ParameterScope(this);
        }

        public void BeginBatch(ProfiledCommand query) {
            foreach(var item in parameters)
                item.Inject(query);
        }         

        public void BeginQuery(ProfiledCommand query) { }

        public void BeginRow(ProfiledDataReader reader) { }

        public void EndBatch(ProfiledCommand query, TimeSpan elapsed) { }

        public void EndQuery(ProfiledCommand query, TimeSpan elapsed) { }

        public void EndRow(ProfiledDataReader reader, TimeSpan elapsed) { }
    }

    [Table(Name = "#MyTable")]
    class MyTable
    {
        [Column]
        public string Value;
    }

    class Program
    {
        static void Main(string[] args) {
            var tempTables = new TableParamaters();
            var session = new DbProfilingSession(tempTables);
            
            var dataContext = new DataContext(session.Connect(new SqlConnection("Server=.;Integrated Security=SSPI")));

            using(tempTables.Add(new[] {
                new MyTable { Value = "Hello " },
                new MyTable { Value = "Temp" },
                new MyTable { Value = "Table" },
                new MyTable { Value = " World!\n" }
            }, x => x.Value)) {
                foreach(var row in dataContext.GetTable<MyTable>())
                    Console.Write("{0}", row.Value);
            }
        }
    }
}
