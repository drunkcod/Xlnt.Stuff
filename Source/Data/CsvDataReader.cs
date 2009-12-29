using System;
using System.Data;
using System.IO;
using Xlnt.IO;

namespace Xlnt.Data
{
    public class CsvDataReader : IDataReader
    {
        class LineReader : ILineReader
        {
            readonly TextReader reader;
            public LineReader(TextReader reader){
                this.reader =reader;
            }

            public string ReadLine() { return reader.ReadLine(); }
            public void Dispose(){ reader.Dispose();}
        }

        static readonly char[] Separators = new[]{','};

        readonly ILineReader reader;
        string[] values;
        string[] fields;
        
        public CsvDataReader(TextReader reader): this(new LineReader(reader)){}

        public CsvDataReader(ILineReader reader){
            this.reader = reader;
        }

        public void ReadHeader(){
            fields = ReadLine();
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
            values = ReadLine();
            return values.Length > 0;
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        void IDisposable.Dispose(){
            reader.Dispose();
        }

        #region IDataRecord Members

        public int FieldCount
        {
            get { return fields.Length; }
            set { fields = new string[value];}
        }

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

        public string GetName(int i){
            return fields[i];
        }

        public int GetOrdinal(string name)
        {
            for(var i = 0; i != fields.Length; ++i)
                if(fields[i] == name)
                    return i;
            for (var i = 0; i != fields.Length; ++i)
                if (string.Compare(fields[i], name, true) == 0)
                    return i;
            throw new ArgumentException("Invalid field name: " + name);
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return values[i];
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

        string[] ReadLine(){
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return new string[0];
            return line.Split(Separators);
        }
    }
}