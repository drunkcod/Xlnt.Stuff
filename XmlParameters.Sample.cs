using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using Xlnt.Data;

namespace ConsoleApplication23
{
    class TableParamaters : IProfilingSessionQueryListener
    {
        interface IParameterInjector
        {
            void Inject(IDbCommand command);
        }

        class ParameterScope : IDisposable
        {
            readonly TableParamaters parent;

            public ParameterScope(TableParamaters parent) {
                this.parent = parent;
            }

            void IDisposable.Dispose() {
                parent.injectors.Pop();
            }
        }

        readonly Stack<Action<IDbCommand>> injectors = new Stack<Action<IDbCommand>>();

        public IDisposable Add<T>(IEnumerable<T> values) {
            var newParameter = new XmlParameter<T>("@" + typeof(T).Name);
            newParameter.AddRange(values);
            injectors.Push(newParameter.Inject);
            return new ParameterScope(this);
        }

        public void BeginBatch(ProfiledCommand query) {
            foreach(var item in injectors)
                item(query);
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

            using(tempTables.Add(GenerateJunk(5))) {
                foreach(var row in dataContext.GetTable<MyTable>())
                    Console.Write("{0}", row.Value);
            }
        }

        static IEnumerable<MyTable> GenerateJunk(int count) {
            for(var i = 0; i != count; ++i)
                yield return new MyTable { Value = i.ToString() };
        }
    }
}
