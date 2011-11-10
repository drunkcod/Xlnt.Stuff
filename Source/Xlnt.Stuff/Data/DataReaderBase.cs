using System;
using System.Collections.Generic;
using System.Data;

namespace Xlnt.Data
{
    public abstract class DataReaderBase : IDataReader
    {
        public abstract int FieldCount { get; }
        public abstract object GetValue(int i);
        public abstract string GetName(int i);
        public int GetOrdinal(string name) {
            for(var i = 0; i != FieldCount; ++i)
                if(GetName(i) == name)
                    return i;
            for(var i = 0; i != FieldCount; ++i)
                if(string.Compare(GetName(i), name, true) == 0)
                    return i;
            throw new IndexOutOfRangeException("Invalid field name: " + name);
        }
        
        public abstract bool Read();

        #region IDataReader Members

        void IDataReader.Close() {
            throw new NotImplementedException();
        }

        int IDataReader.Depth {
            get { throw new NotImplementedException(); }
        }

        DataTable IDataReader.GetSchemaTable() {
            throw new NotImplementedException();
        }

        bool IDataReader.IsClosed {
            get { throw new NotImplementedException(); }
        }

        bool IDataReader.NextResult() {
            throw new NotImplementedException();
        }

        int IDataReader.RecordsAffected {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() { DisposeCore(); }

        #endregion

        #region IDataRecord Members

        bool IDataRecord.GetBoolean(int i) {
            throw new NotImplementedException();
        }

        byte IDataRecord.GetByte(int i) {
            throw new NotImplementedException();
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            throw new NotImplementedException();
        }

        char IDataRecord.GetChar(int i) {
            throw new NotImplementedException();
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            throw new NotImplementedException();
        }

        IDataReader IDataRecord.GetData(int i) {
            throw new NotImplementedException();
        }

        string IDataRecord.GetDataTypeName(int i) {
            throw new NotImplementedException();
        }

        DateTime IDataRecord.GetDateTime(int i) {
            throw new NotImplementedException();
        }

        decimal IDataRecord.GetDecimal(int i) {
            throw new NotImplementedException();
        }

        double IDataRecord.GetDouble(int i) {
            throw new NotImplementedException();
        }

        Type IDataRecord.GetFieldType(int i) {
            throw new NotImplementedException();
        }

        float IDataRecord.GetFloat(int i) {
            throw new NotImplementedException();
        }

        Guid IDataRecord.GetGuid(int i) {
            throw new NotImplementedException();
        }

        short IDataRecord.GetInt16(int i) {
            throw new NotImplementedException();
        }

        int IDataRecord.GetInt32(int i) { return Convert.ToInt32(GetValue(i)); }

        long IDataRecord.GetInt64(int i) {
            throw new NotImplementedException();
        }

        string IDataRecord.GetString(int i) { return GetValue(i).ToString(); }

        int IDataRecord.GetValues(object[] values) {
            throw new NotImplementedException();
        }

        bool IDataRecord.IsDBNull(int i) {
            throw new NotImplementedException();
        }

        object IDataRecord.this[string name] {
            get { return GetValue(GetOrdinal(name)); }
        }

        object IDataRecord.this[int i] {
            get { throw new NotImplementedException(); }
        }

        #endregion

        protected virtual void DisposeCore(){}
    }
}