using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using Xlnt.Data;

namespace ConsoleApplication23
{
    [Table(Name = "#MyTable")]
    class MyTable
    {
        [Column]
        public string Value;
    }

    class Program
    {
        static void Main(string[] args) {
            var tempTables = new TableParameters();
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
