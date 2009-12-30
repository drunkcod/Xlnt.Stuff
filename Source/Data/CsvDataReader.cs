using System;
using System.Data;
using System.IO;
using Xlnt.IO;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public class CsvDataReader : IDataReader
    {
        static readonly char[] DefaultSeparator = new[]{','};

        readonly ILineReader reader;
        string[] values;
        string[] fields;
        char[] separator = DefaultSeparator;
        
        public CsvDataReader(TextReader reader): this(new LineReader(reader)){}

        public CsvDataReader(ILineReader reader){
            this.reader = reader;
        }

        public int FieldCount {
            get { return fields.Length; }
            set { fields = new string[value]; }
        }

        public char Separator
        {
            get { return separator[0]; }
            set { separator = new[] {value}; }
        }

        public void ReadHeader(){
            fields = ReadLine();
        }

        public bool Read(){
            values = ReadLine();
            return values.Length > 0;
        }

        public string GetName(int i){
            return fields[i];
        }

        public bool HasField(string name){
            return fields.Any(item => string.Compare(item, name, true) == 0);
        }

        public int GetOrdinal(string name){
            for (var i = 0; i != fields.Length; ++i)
                if (fields[i] == name)
                    return i;
            for (var i = 0; i != fields.Length; ++i)
                if (string.Compare(fields[i], name, true) == 0)
                    return i;
            throw new ArgumentException("Invalid field name: " + name);
        }

        public object GetValue(int i){
            return values[i];
        }

        #region IDataReader Members

        void IDataReader.Close(){
            throw new NotImplementedException();
        }

        int IDataReader.Depth {
            get { throw new NotImplementedException(); }
        }

        DataTable IDataReader.GetSchemaTable(){
            throw new NotImplementedException();
        }

        bool IDataReader.IsClosed {
            get { throw new NotImplementedException(); }
        }

        bool IDataReader.NextResult(){
            throw new NotImplementedException();
        }

        int IDataReader.RecordsAffected {
            get { throw new NotImplementedException(); }
        }

        #endregion

        void IDisposable.Dispose(){
            reader.Dispose();
        }

        #region IDataRecord Members

        bool IDataRecord.GetBoolean(int i){
            throw new NotImplementedException();
        }

        byte IDataRecord.GetByte(int i){
            throw new NotImplementedException();
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length){
            throw new NotImplementedException();
        }

        char IDataRecord.GetChar(int i){
            throw new NotImplementedException();
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length){
            throw new NotImplementedException();
        }

        IDataReader IDataRecord.GetData(int i){
            throw new NotImplementedException();
        }

        string IDataRecord.GetDataTypeName(int i){
            throw new NotImplementedException();
        }

        DateTime IDataRecord.GetDateTime(int i){
            throw new NotImplementedException();
        }

        decimal IDataRecord.GetDecimal(int i){
            throw new NotImplementedException();
        }

        double IDataRecord.GetDouble(int i){
            throw new NotImplementedException();
        }

        Type IDataRecord.GetFieldType(int i){
            throw new NotImplementedException();
        }

        float IDataRecord.GetFloat(int i){
            throw new NotImplementedException();
        }

        Guid IDataRecord.GetGuid(int i){
            throw new NotImplementedException();
        }

        short IDataRecord.GetInt16(int i){
            throw new NotImplementedException();
        }

        int IDataRecord.GetInt32(int i){
            throw new NotImplementedException();
        }

        long IDataRecord.GetInt64(int i){
            throw new NotImplementedException();
        }

        string IDataRecord.GetString(int i){
            throw new NotImplementedException();
        }

        int IDataRecord.GetValues(object[] values){
            throw new NotImplementedException();
        }

        bool IDataRecord.IsDBNull(int i){
            throw new NotImplementedException();
        }

        object IDataRecord.this[string name] {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[int i] {
            get { throw new NotImplementedException(); }
        }

        #endregion

        string[] ReadLine(){
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return new string[0];
            return line.Split(separator);
        }
    }
}