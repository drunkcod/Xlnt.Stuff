using System;
using System.IO;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xlnt.Data;
using Xlnt.IO;
using System.Data;

namespace Xlnt.Data
{
    public class CsvDataReaderTests
    {
        [Test]
        public void cant_Read_from_empty_stream(){
            var csv = CsvDataReader.Parse(string.Empty);
            Assert.That(csv.Read(), Is.False);
        }
        [Test]
        public void GetValue_returns_fields_in_order(){
            var csv = CsvDataReader.Parse("1,2,3");
            csv.Read();

            Assert.That(new[]{ csv.GetValue(0), csv.GetValue(1), csv.GetValue(2)}, Is.EqualTo(new[]{"1", "2", "3"}));            
        }
        [Test]
        public void determines_field_count_from_header_row(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();
            
            Assert.That(csv.FieldCount, Is.EqualTo(2));
        }
        [Test]
        public void determines_field_names_from_header_row(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();

            Assert.That(new[]{csv.GetName(0), csv.GetName(1)}, Is.EqualTo(new[]{"Id", "Value"}));
        }
        [Test]
        public void throws_InvalidOperationExcetion_if_ReadHeader_or_SetFieldCount_not_called_before_FieldCount_is_used() {
            var csv = CsvDataReader.Parse("Id,Value");

            Assert.Throws(typeof(InvalidOperationException), () => { var x = csv.FieldCount; });
        }
        [Test]
        public void field_ordinals_match_header_ordering(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();

            Assert.That(new[] { csv.GetOrdinal("Id"), csv.GetOrdinal("Value") }, Is.EqualTo(new[] { 0, 1 }));            
        }
        [Test]
        public void handle_escaped_fields() {
            var csv = CsvDataReader.Parse("\",\\\"");
            csv.ReadHeader();

            Assert.That(csv.GetName(0), Is.EqualTo(",\""));
        }
        [Test]
        public void field_ordinals_give_predecence_to_case_sensative_matches(){
            var csv = CsvDataReader.Parse("id,Id");
            csv.ReadHeader();
            Assert.That(csv.GetOrdinal("Id"), Is.EqualTo(1));            
        }
        [Test]
        public void field_ordinals_support_case_ignorant_matchin(){
            var csv = CsvDataReader.Parse("id");
            csv.ReadHeader();
            Assert.That(csv.GetOrdinal("Id"), Is.EqualTo(0));
        }
        [Test]
        public void unknown_fields_throw_exception_when_getting_ordinal(){
            var csv = CsvDataReader.Parse("id");
            csv.ReadHeader();
            Assert.Throws(typeof(IndexOutOfRangeException), () => csv.GetOrdinal("MissingField"));            
        }
        [Test]
        public void should_support_different_separators(){
            var csv = CsvDataReader.Parse("1;2", ';');
            csv.Read();
            Assert.That(new[] {csv.GetValue(0), csv.GetValue(1)}, Is.EqualTo(new[] {"1", "2"}));
        }
        [Test]
        public void supports_checking_if_field_exists(){
            var csv = CsvDataReader.Parse("First,Second");
            csv.ReadHeader();
            Assert.That(new[] { csv.HasField("First"), csv.HasField("NotAvailable") }, Is.EqualTo(new[] { true, false }));
        }
        [Test]
        public void field_existance_check_should_be_case_insensitive(){
            var csv = CsvDataReader.Parse("First");
            csv.ReadHeader();
            Assert.That(csv.HasField("fiRst"), Is.True);
        }
        [Test]
        public void should_support_multiple_data_rows() {
            var csv = CsvDataReader.Parse("First\r\n1\n2");
            csv.ReadHeader();
            IDataRecord record = csv;
            csv.Read();
            Assert.That(record["First"], Is.EqualTo("1"), "first line");
            csv.Read();
            Assert.That(record["First"], Is.EqualTo("2"), "second line");
        }
        [Test]
        public void should_stop_when_out_of_data() {
            var csv = CsvDataReader.Parse("First\r\n1\n2");
            csv.ReadHeader();
            Assert.That(new[] {
                csv.Read(), csv.Read(), csv.Read()
            }, Is.EqualTo(new []{ true, true, false}));
        }

    }
}