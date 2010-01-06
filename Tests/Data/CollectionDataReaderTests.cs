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
    public class CollectionDataReaderTests : SqlBulkCopyFixture
    {
        readonly Row[] SomeRows = new[]{                   
            new Row { Id = 42, Value = "The Answer" }, 
            new Row { Id = 7, Value = "Sins" } };

        [Test]
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
        [Test]
        public void contiains_source_number_of_rows() {
            IDataReader data = new CollectionDataReader<Row>(SomeRows);

            var count = 0;
            while(data.Read())
                ++count;

            Assert.That(count, Is.EqualTo(SomeRows.Length));
        }
    }
}
