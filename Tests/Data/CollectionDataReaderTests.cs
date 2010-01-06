using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using Xlnt.Data;

namespace Xlnt.Tests.Data
{
    class CollectionDataReader<T> : IDataReader
    {
        readonly IEnumerable<T> source;
        FieldCollection<T> columns = new FieldCollection<T>();

        public CollectionDataReader(IEnumerable<T> source){
            this.source = source;
        }

        public FieldCollection<T> ColumnMappings 
        {
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
        readonly Row[] SomeRows = new[]{                   
            new Row { Id = 42, Value = "The Answer" }, 
            new Row { Id = 7, Value = "Sins" } };


        [Test, Pending]
        public void should_be_SqlBulkCopy_compatible() {
            WithConnection(db => {
                var bulkCopy = SqlBulkCopyForRows(db);

                var data = new CollectionDataReader<Row>(SomeRows);
                data.ColumnMappings
                    .Add(x => x.Id)
                    .Add(x => x.Value);

                bulkCopy.WriteToServer(data);
                CheckRows(db, SomeRows);
            });
        }
        [Test]
        public void should_use_field_count_from_FieldCollection() {
            var fields = new FieldCollection<Row>()
                .Add(x => x.Id)
                .Add(x => x.Value);

            var data = new CollectionDataReader<Row>(SomeRows);
            data.ColumnMappings = fields;

            Assert.That((data as IDataReader).FieldCount, Is.EqualTo(2));
        }
    }
}
