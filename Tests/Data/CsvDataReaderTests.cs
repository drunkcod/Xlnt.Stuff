using System;
using System.IO;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xlnt.Data;
using Xlnt.IO;

namespace Xlnt.Tests.Data
{
    public class CsvDataReaderTests
    {
        [Test]
        public void cant_Read_from_empty_stream(){
            var csv = new CsvDataReader(new StringReader(string.Empty));
            Assert.That(csv.Read(), Is.False);
        }
        [Test]
        public void GetValue_returns_fields_in_order(){
            var csv = new CsvDataReader(new StringReader("1,2,3"));
            csv.Read();

            Assert.That(new[]{ csv.GetValue(0), csv.GetValue(1), csv.GetValue(2)}, Is.EqualTo(new[]{"1", "2", "3"}));            
        }
        [Test]
        public void determines_field_count_from_header_row(){
            var csv = new CsvDataReader(new StringReader("Id,Value"));
            csv.ReadHeader();
            
            Assert.That(csv.FieldCount, Is.EqualTo(2));
        }
        [Test]
        public void determines_field_names_from_header_row(){
            var csv = new CsvDataReader(new StringReader("Id,Value"));
            csv.ReadHeader();

            Assert.That(new[]{csv.GetName(0), csv.GetName(1)}, Is.EqualTo(new[]{"Id", "Value"}));
        }
        [Test]
        public void field_ordinals_match_header_ordering(){
            var csv = new CsvDataReader(new StringReader("Id,Value"));
            csv.ReadHeader();

            Assert.That(new[] { csv.GetOrdinal("Id"), csv.GetOrdinal("Value") }, Is.EqualTo(new[] { 0, 1 }));            
        }
        [Test]
        public void field_ordinals_give_predecence_to_case_sensative_matches(){
            var csv = new CsvDataReader(new StringReader("id,Id"));
            csv.ReadHeader();
            Assert.That(csv.GetOrdinal("Id"), Is.EqualTo(1));            
        }
        [Test]
        public void field_ordinals_support_case_ignorant_matchin(){
            var csv = new CsvDataReader(new StringReader("id"));
            csv.ReadHeader();
            Assert.That(csv.GetOrdinal("Id"), Is.EqualTo(0));
        }
        [Test]
        public void unknown_fields_throw_exception_when_getting_ordinal(){
            var csv = new CsvDataReader(new StringReader("id"));
            csv.ReadHeader();
            Assert.Throws(typeof(ArgumentException), () => csv.GetOrdinal("MissingField"));            
        }
        [Test]
        public void should_dispose_line_reader(){
            var mock = new Mock<ILineReader>();
            using(new CsvDataReader(mock.Object))
                ;            
            mock.Verify(x => x.Dispose());
        }
        [Test]
        public void should_support_different_separators(){
            var csv = new CsvDataReader(new StringReader("1;2")) {Separator = ';'};
            csv.Read();
            Assert.That(new[] {csv.GetValue(0), csv.GetValue(1)}, Is.EqualTo(new[] {"1", "2"}));
        }
        [Test]
        public void supports_checking_if_field_exists(){
            var csv = new CsvDataReader(new StringReader("First,Second"));
            csv.ReadHeader();
            Assert.That(new[] { csv.HasField("First"), csv.HasField("NotAvailable") }, Is.EqualTo(new[] { true, false }));
        }
    }
}