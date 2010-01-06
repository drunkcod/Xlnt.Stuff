using System;
using System.Collections.Generic;
using System.Data;

namespace Xlnt.Data
{
   public  class CollectionDataReader<T> : IDataReader
    {
        readonly IEnumerator<T> items;
        FieldCollection<T> columns = new FieldCollection<T>();

        public CollectionDataReader(IEnumerable<T> source) {
            items = source.GetEnumerator();
        }

        public FieldCollection<T> ColumnMappings {
            get { return columns; }
            set { columns = value; }
        }

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

        bool IDataReader.Read() { return items.MoveNext(); }

        int IDataReader.RecordsAffected {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataRecord Members

        int IDataRecord.FieldCount { get { return columns.Count; } }

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

        int IDataRecord.GetInt32(int i) {
            throw new NotImplementedException();
        }

        long IDataRecord.GetInt64(int i) {
            throw new NotImplementedException();
        }

        string IDataRecord.GetName(int i) {
            throw new NotImplementedException();
        }

        int IDataRecord.GetOrdinal(string name) {
            throw new NotImplementedException();
        }

        string IDataRecord.GetString(int i) {
            throw new NotImplementedException();
        }

        object IDataRecord.GetValue(int i) { return columns.Read(items.Current, i); }

        int IDataRecord.GetValues(object[] values) {
            throw new NotImplementedException();
        }

        bool IDataRecord.IsDBNull(int i) {
            throw new NotImplementedException();
        }

        object IDataRecord.this[string name] {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[int i] {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
