using System;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using Cone;

namespace Xlnt.Data
{
    [Describe(typeof(DbProfiler))]
    public class DbProfilerSpec
    {
        static string DataPath { get { return Path.GetDirectoryName(new Uri(typeof(DbProfilerSpec).Assembly.CodeBase).LocalPath); } }

        static DbConnection OpenSampleConnection() {
            var connection = new SqlCeConnection(string.Format("DataSource={0}", Path.Combine(DataPath, "Sample.sdf")));
            connection.Open();
            return connection;
        }

        [DisplayAs("Ado.Net usage")]
        public void basic_usage() {
            var session = new DbProfilingSession();
            var db = DbProfiler.Connect(session, OpenSampleConnection());

            var query = db.CreateCommand();
            query.CommandText = "select sum(value) from Numbers";
            query.ExecuteScalar();

            Verify.That(() => session.QueryCount == 1);
            Verify.That(() => session.RowCount == 0);
        }

        [Context("Linq2Sql usage")]
        public class Linq2Sql 
        {
            //Sample Table.
            [Table(Name = "Numbers")]
            public class Number
            {
                [Column(Name = "Value")]
                public int Value;    
            }

            int NumbersRowCount { 
                get { 
                    using(var db = OpenSampleConnection())
                        return (int)db.ExecuteScalar("select count(*) from Numbers");
                }
            }

            public void compare_deferred_and_local_execution() {
                var session = new DbProfilingSession();
                var context = new DataContext(DbProfiler.Connect(session, OpenSampleConnection()));
                var numbers = context.GetTable<Number>(); 

                var deffered = session.Scoped("Sent to database for execution", _ => {
                    numbers.Sum(x => x.Value);
                });                    

                var inMemory = session.Scoped("Pull all rows to memory", _ => {
                    numbers.AsEnumerable().Sum(x => x.Value);                   
                });

                Verify.That(() => deffered.QueryCount == 1);
                Verify.That(() => deffered.RowCount == 1);

                Verify.That(() => inMemory.QueryCount == 1);
                Verify.That(() => inMemory.RowCount == NumbersRowCount);

                Verify.That(() => session.QueryCount == deffered.QueryCount + inMemory.QueryCount);
                Verify.That(() => session.RowCount == deffered.RowCount + inMemory.RowCount);
            }

            [Context("query rewriting")]
            public class QueryRewriting
            {
                public void sample() {
                    var rewrite = new LinqQueryRewritingSession();
                    var trace = new TracingEventProfilingSessionQueryListener(rewrite);
                    trace.EndQuery += (s, e) => {
                        Console.WriteLine("({0}) {1}", e.Elapsed, e.CommandText);
                    };

                    var session = new DbProfilingSession(trace, rewrite);
                    using(var db = OpenSampleConnection()) {
                        var context = new DataContext(DbProfiler.Connect(session, db));
                        var numbers = context.GetTable<Number>(); 
                        var tableName = context.Mapping.GetTable(typeof(Number)).TableName;
                        session.Scoped("#nolock;" + tableName, scope => {
                            numbers.Count();
                        });

                        rewrite.PushNoLockScope(tableName);
                        numbers.Count();
                        rewrite.PopNoLockScope();
                        numbers.Count();
                    }
                }
            }
        }
    }
}
