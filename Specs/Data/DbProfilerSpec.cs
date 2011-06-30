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
    [Table(Name = "Numbers")]
    public class Number
    {
        [Column(Name = "Value")]
        public int Value;    
    }

    class MyProfilingSession : DbProfilingSession 
    {
        public override void BeginQuery(ProfiledCommand query) {
            Console.WriteLine(query.CommandText);
        }

        public override void BeginRow(ProfiledDataReader reader) {
            Console.WriteLine("READING");
        }
    }

    [Describe(typeof(DbProfiler))]
    public class DbProfilerSpec
    {
        string DataPath { get { return Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath); } }

        DbConnection NewSampleConnection() {
            return new SqlCeConnection(string.Format("DataSource={0}", Path.Combine(DataPath, "Sample.sdf")));
        }

        [DisplayAs("Ado.Net usage")]
        public void basic_usage() {
            var session = new DbProfilingSession();
            var profiler = new DbProfiler();
            var db = profiler.Connect(session, NewSampleConnection());

            var query = db.CreateCommand();
            query.CommandText = "select sum(value) from Numbers";

            db.Open();
            query.ExecuteScalar();

            Verify.That(() => session.QueryCount == 1);
            Verify.That(() => session.RowCount == 0);
        }

        public void Linq2Sql_usage() {
            var session = new MyProfilingSession();
            var profiler = new DbProfiler();
            var db = profiler.Connect(session, NewSampleConnection());

            var context = new DataContext(db);
            var numbers = context.GetTable<Number>(); 
            //Send it to the database for execution
            numbers.Sum(x => x.Value);
            Verify.That(() => session.QueryCount == 1);
            Verify.That(() => session.RowCount == 1);

            var numbersRowCount = numbers.Count();
            session.Reset();
            //Force in memory execution
            context.GetTable<Number>().AsEnumerable().Sum(x => x.Value);
            Verify.That(() => session.QueryCount == 1);
            Verify.That(() => session.RowCount == numbersRowCount);
        
        }
    }
}
