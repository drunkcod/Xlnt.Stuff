using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using Xlnt.Data;
using Xlnt.NUnit;

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
            var data = new CollectionDataReader<Row>(SomeRows);
                data.ColumnMappings
                    .Add(x => (object)x.Id)
                    .Add(x => x.Value);

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

        class TypeWithFieldsAndProperties
        {
            public int SomeField;
            int PrivateField;
            public int SomeProperty { get { return PrivateField; } set { PrivateField = value; } }
        }

        [TestCaseSource("FieldMappingTests")]
        public void Scenarios(Action act){ act(); }

        public Scenario FieldMappingTests() {
            return new Scenario()
            .Given("a CollectionDataReader for a type with fields and propperties", () =>
                new[] { new TypeWithFieldsAndProperties { SomeField = 1, SomeProperty = 2} }.AsDataReader())
                .When("I MapAll", x => { x.MapAll(); x.Read(); })
                .Then("fields are mapped", x => Assert.True(x.ColumnMappings.Any(field => field.Name == "SomeField")))
                .And("fields are readable", x => Assert.That(x.GetValue(x.GetOrdinal("SomeField")), Is.EqualTo(1)))
                .And("properties are mapped", x => Assert.True(x.ColumnMappings.Any(field => field.Name == "SomeProperty")))
                .And("properties are readable", x => Assert.That(x.GetValue(x.GetOrdinal("SomeProperty")), Is.EqualTo(2)))
                .And("the number of columns matches public fields + properties", x => Assert.That(x.ColumnMappings.Count, Is.EqualTo(2)));
        }
    }
}