using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Xlnt.Data;
using Cone;

namespace Xlnt.Data
{
    public class Row
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

        public override string ToString() {
            return string.Format("{{\"Id\":{0},\"Value\":\"{1}\"}}", Id, Value);
        }
    }

	[Feature("CsvBulkCopy")]
    public class CsvBulkCopyTests : SqlBulkCopyFixture
    {
        public void Insert_multiple_rows_to_tempdb(){
            WithConnection(db => {
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("42,The Answer\r\n7,Sins"));
                data.SetFieldCount(2);

                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row { Id = 42, Value = "The Answer" }, new Row { Id = 7, Value = "Sins" });
            });
        }

		public void supports_ColumnMapping_by_ordinal(){
            WithConnection(db => {
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("The Answer, 42\r\nSins, 7"));
                data.SetFieldCount(2);

                bulkCopy.ColumnMappings.Add(0, 1);
                bulkCopy.ColumnMappings.Add(1, 0);
                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row { Id = 42, Value = "The Answer" }, new Row { Id = 7, Value = "Sins" });
            });
        }

		public void supports_ColumnMapping_by_name()
        {
            WithConnection(db => {
                var bulkCopy = SqlBulkCopyForRows(db);
                var data = new CsvDataReader(new StringReader("Value,Id\r\nThe Answer, 42\r\nSins, 7"));
                data.SetFieldCount(2);

                data.ReadHeader();
                bulkCopy.ColumnMappings.Add("Value", "value");
                bulkCopy.ColumnMappings.Add("Id", "id");
                bulkCopy.WriteToServer(data);

                CheckRows(db, new Row { Id = 42, Value = "The Answer" }, new Row { Id = 7, Value = "Sins" });
            });
        }
    }
}