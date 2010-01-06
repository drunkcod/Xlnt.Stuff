using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;

namespace Xlnt.Tests.Data
{
    class CollectionDataReader<T> : IDataReader
    {
        readonly IEnumerable<T> source;

        public CollectionDataReader(IEnumerable<T> source){
            this.source = source;
        }

        public CollectionDataReader<T> Map<TAny>(Expression<Func<T,TAny>> column){ return this; }

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

        bool IDataReader.Read() {
            throw new NotImplementedException();
        }

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

        int IDataRecord.FieldCount {
            get { throw new NotImplementedException(); }
        }

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

        object IDataRecord.GetValue(int i) {
            throw new NotImplementedException();
        }

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

    public class PendingAttribute : CategoryAttribute 
    {
        public PendingAttribute() : base("Pending") { }
    }

    public class CollectionDataReaderTests : SqlBulkCopyFixture
    {
        [Test, Pending]
        public void should_be_SqlBulkCopy_compatible() {
            WithConnection(db => {
                var bulkCopy = SqlBulkCopyForRows(db);

                var rows = new[]{                   
                    new Row { Id = 42, Value = "The Answer" }, 
                    new Row { Id = 7, Value = "Sins" } };

                var data = new CollectionDataReader<Row>(rows)
                    .Map(x => x.Id)
                    .Map(x => x.Value);

                bulkCopy.WriteToServer(data);
                CheckRows(db, rows);
            });
        }
    }
}
