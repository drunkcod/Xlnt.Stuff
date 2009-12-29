using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;

namespace Xlnt.Tests.Data
{
    class CsvDataReader : IDataReader
    {
        readonly TextReader reader;
        string line;
        
        public CsvDataReader(TextReader reader){
            this.reader = reader;    
        }

        #region IDataReader Members

        public void Close()
        {
            throw new NotImplementedException();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            line = reader.ReadLine();
            return !string.IsNullOrEmpty(line);
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount { get; set; }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    class Row
    {
        public int Id;
        public string Value;
    }

    public class CsvBulkCopyTests
    {
        private const string ConnectionString = "Server=.;Integrated Security=SSPI";
        [Test]
        public void Insert_multiple_rows_to_tempdb()
        {
            using(var db = new SqlConnection(ConnectionString))
            using(var command = db.CreateCommand())
            {
                db.Open();
                command.CommandText = "create table #rows(id int,value varchar(max))";
                command.ExecuteNonQuery();
                var bulkCopy = new SqlBulkCopy(ConnectionString) {DestinationTableName = "tempdb..#rows"};

                var data = new CsvDataReader(new StringReader("42,The Answer\r\n7,Sins"));

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

    public class CsvDataReaderTests
    {
        [Test]
        public void cant_Read_from_empty_stream()
        {
            var csv = new CsvDataReader(new StringReader(string.Empty));
            Assert.That(csv.Read(), Is.False);
        }
    }
}
