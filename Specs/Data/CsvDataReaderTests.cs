using System;
using System.IO;
using Moq;
using Xlnt.Data;
using Xlnt.IO;
using System.Data;
using Cone;

namespace Xlnt.Data
{
	[Describe(typeof(CsvDataReader))]
    public class CsvDataReaderTests
    {
        public void cant_Read_from_empty_stream(){
            var csv = CsvDataReader.Parse(string.Empty);
            Verify.That(() => csv.Read() == false);
        }

		public void GetValue_returns_fields_in_order(){
            var csv = CsvDataReader.Parse("1,2,3");
            csv.Read();

			Verify.That(() => csv.GetValue(0) == "1");            
			Verify.That(() => csv.GetValue(1) == "2");            
			Verify.That(() => csv.GetValue(2) == "3");            
        }

		public void determines_field_count_from_header_row(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();
            
            Verify.That(() => csv.FieldCount == 2);
        }

		public void determines_field_names_from_header_row(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();

			Verify.That(() => csv.GetName(0) == "Id");
			Verify.That(() => csv.GetName(1) == "Value");
        }

		public void throws_InvalidOperationExcetion_if_ReadHeader_or_SetFieldCount_not_called_before_FieldCount_is_used() {
            var csv = CsvDataReader.Parse("Id,Value");
            Verify.Throws<InvalidOperationException>.When(() => csv.FieldCount);
        }

		public void field_ordinals_match_header_ordering(){
            var csv = CsvDataReader.Parse("Id,Value");
            csv.ReadHeader();

			Verify.That(() => csv.GetOrdinal("Id") == 0);
			Verify.That(() => csv.GetOrdinal("Value") == 1);
        }

		public void handle_escaped_fields() {
            var csv = CsvDataReader.Parse("\",\\\"");
            csv.ReadHeader();

            Verify.That(() => csv.GetName(0) == ",\"");
        }

		public void field_ordinals_give_predecence_to_case_sensative_matches(){
            var csv = CsvDataReader.Parse("id,Id");
            csv.ReadHeader();
            Verify.That(() => csv.GetOrdinal("Id") == 1);            
        }

		public void field_ordinals_support_case_ignorant_matchin(){
            var csv = CsvDataReader.Parse("id");
            csv.ReadHeader();
            Verify.That(() => csv.GetOrdinal("Id") == 0);
        }

		public void unknown_fields_throw_exception_when_getting_ordinal(){
            var csv = CsvDataReader.Parse("id");
            csv.ReadHeader();
            Verify.Throws<IndexOutOfRangeException>.When(() => csv.GetOrdinal("MissingField"));            
        }

		public void should_support_different_separators(){
            var csv = CsvDataReader.Parse("1;2", ';');
            csv.Read();

			Verify.That(() => csv.GetValue(0) == "1");
			Verify.That(() => csv.GetValue(1) == "2");
        }

		public void supports_checking_if_field_exists(){
            var csv = CsvDataReader.Parse("First,Second");
            csv.ReadHeader();

			Verify.That(() => csv.HasField("First"));
			Verify.That(() => csv.HasField("NotAvailable") == false);
        }

		public void field_existance_check_should_be_case_insensitive(){
            var csv = CsvDataReader.Parse("First");
            csv.ReadHeader();
            Verify.That(() => csv.HasField("fiRst"));
        }

		public void should_support_multiple_data_rows() {
            var csv = CsvDataReader.Parse("First\r\n1\n2");
            csv.ReadHeader();
            IDataRecord record = csv;
            csv.Read();
            Verify.That(() => record["First"] == "1");
            csv.Read();
            Verify.That(() => record["First"] == "2");
        }

		public void should_stop_when_out_of_data() {
            var csv = CsvDataReader.Parse("First\r\n1\n2");
            csv.ReadHeader();

			Verify.That(() => csv.Read());
			Verify.That(() => csv.Read());
			Verify.That(() => csv.Read() == false);
        }
    }
}