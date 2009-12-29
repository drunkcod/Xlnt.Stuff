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
        private const string ConnectionString = "Server=.;Integrated Security=SSPI";
        
        [Test]
        public void Insert_multiple_rows_to_tempdb(){
            using (var db = new SqlConnection(ConnectionString)){
                db.Open();
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("42,The Answer\r\n7,Sins")) {FieldCount = 2};

                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row { Id = 42, Value = "The Answer" }, new Row { Id = 7, Value = "Sins" });
            }
        }
        [Test]
        public void supports_ColumnMapping_by_ordinal(){
            using (var db = new SqlConnection(ConnectionString)){
                db.Open();
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("The Answer, 42\r\nSins, 7")) { FieldCount = 2 };

                bulkCopy.ColumnMappings.Add(0, 1);
                bulkCopy.ColumnMappings.Add(1, 0);
                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row { Id = 42, Value = "The Answer" }, new Row { Id = 7, Value = "Sins" });
            }            
        }
        [Test]
        public void supports_ColumnMapping_by_name()
        {
            using (var db = new SqlConnection(ConnectionString)){
                db.Open();
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("Value,Id\r\nThe Answer, 42\r\nSins, 7")) {FieldCount = 2};

                data.ReadHeader();
                bulkCopy.ColumnMappings.Add("Value", "value");
                bulkCopy.ColumnMappings.Add("Id", "id");
                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row {Id = 42, Value = "The Answer"}, new Row {Id = 7, Value = "Sins"});
            }
        }

        static SqlBulkCopy SqlBulkCopyForRows(SqlConnection db)
        {
            using (var command = db.CreateCommand()){
                command.CommandText = "create table #rows(id int,value varchar(max))";
                command.ExecuteNonQuery();
            }
            return new SqlBulkCopy(db) { DestinationTableName = "#rows" };            
        }

        static void CheckRows(SqlConnection db, params Row[] expected){
            using (var command = db.CreateCommand()){
                command.CommandText = "select id,value from #rows";
                var rows = new List<Row>();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        rows.Add(new Row {Id = reader.GetInt32(0), Value = reader.GetString(1)});
                Assert.That(rows, Is.EqualTo(expected));
            }
        }
    }
}
