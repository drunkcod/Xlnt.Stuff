using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using Xlnt.Data;

namespace Xlnt.Tests.Data
{
    class Row
    {
        public int Id;
        public string Value;

        public override bool Equals(object obj){
            var other = (Row) obj;
            return Id == other.Id && Value == other.Value;
        }

        public override int GetHashCode(){
            return Value.GetHashCode();
        }
    }

    public class CsvBulkCopyTests
    {
        private const string ConnectionString = "Server=.;Integrated Security=SSPI;Initial Catalog=tempdb";
        
        [Test]
        public void Insert_multiple_rows_to_tempdb(){
            using(var db = new SqlConnection(ConnectionString))
            using(var command = db.CreateCommand())
            {
                db.Open();
                command.CommandText = "create table #rows(id int,value varchar(max))";
                command.ExecuteNonQuery();
                var bulkCopy = new SqlBulkCopy(db) {DestinationTableName = "#rows"};

                var data = new CsvDataReader(new StringReader("42,The Answer\r\n7,Sins")) {FieldCount = 2};

                bulkCopy.WriteToServer(data);

                command.CommandText = "select id,value from #rows";
                var rows = new List<Row>();
                using(var reader = command.ExecuteReader())
                    while(reader.Read())
                        rows.Add(new Row{Id = reader.GetInt32(0), Value = reader.GetString(1)});
                Assert.That(rows, Is.EqualTo(new[]
                {
                    new Row{ Id = 42, Value = "The Answer"},
                    new Row{ Id = 7, Value = "Sins"}                
                }));
            }
        }        
    }
}
