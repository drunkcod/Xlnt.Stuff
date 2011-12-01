using System.Data;
using System.Linq;
using Cone;

namespace Xlnt.Data
{
    [Describe(typeof(CollectionDataReader<>))]
    public class CollectionDataReaderTests : SqlBulkCopyFixture
    {
        readonly Row[] SomeRows = new[]{                   
            new Row { Id = 42, Value = "The Answer" }, 
            new Row { Id = 7, Value = "Sins" } };

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

        public void should_use_field_count_from_FieldCollection() {
            var data = new CollectionDataReader<Row>(SomeRows);
                data.ColumnMappings
                    .Add(x => (object)x.Id)
                    .Add(x => x.Value);

            Verify.That(() => (data as IDataReader).FieldCount == 2);
        }

        public void contiains_source_number_of_rows() {
            IDataReader data = new CollectionDataReader<Row>(SomeRows);

            var count = 0;
            while(data.Read())
                ++count;

            Verify.That(() => count == SomeRows.Length);
        }

        class TypeWithFieldsAndProperties
        {
            public int SomeField;
            int PrivateField;
            public int SomeProperty { get { return PrivateField; } set { PrivateField = value; } }
        }

        [Context("Given a CollectionDataReader for a type with fields & properties")]
        public class FieldMapping
        {
            CollectionDataReader<TypeWithFieldsAndProperties> DataReader;
            [BeforeAll]
            public void MapAll() 
            {
                DataReader = new[] { new TypeWithFieldsAndProperties { SomeField = 1, SomeProperty = 2} }.AsDataReader();
                DataReader.MapAll();
                DataReader.Read();
            }

            public void fields_are_mapped() { 
                Verify.That(() => DataReader.ColumnMappings.Any(field => field.Name == "SomeField")); 
            }
            
            public void fields_are__readable() { 
                Verify.That(() => (int)DataReader.GetValue(DataReader.GetOrdinal("SomeField")) == 1); 
            }

            public void properties_are_mapped() { 
                Verify.That(() => DataReader.ColumnMappings.Any(field => field.Name == "SomeProperty")); 
            }

            public void properties_are_readable() {
                Verify.That(() => (int)DataReader.GetValue(DataReader.GetOrdinal("SomeProperty")) == 2);
            }

            public void number_of_columns_matche_public_fields_and_properties() {
                Verify.That(() => DataReader.ColumnMappings.Count == 2);
            }
        }
    }
}