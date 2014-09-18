using System;
using System.Data;
using Cone;

namespace Xlnt.Data
{
	[Describe(typeof(CsvDataReader))]
	public class CsvDataReaderTests
	{
		public void cant_Read_from_empty_stream(){
			var csv = CsvDataReader.Parse(string.Empty);
			Check.That(() => csv.Read() == false);
		}

		public void GetValue_returns_fields_in_order(){
			var csv = CsvDataReader.Parse("1,2,3");
			csv.Read();

			Check.That(
				() => csv.GetValue(0).Equals("1"),
				() => csv.GetValue(1).Equals("2"),
				() => csv.GetValue(2).Equals("3"));
		}

		public void determines_field_count_from_header_row(){
			var csv = CsvDataReader.Parse("Id,Value");
			csv.ReadHeader();

			Check.That(() => csv.FieldCount == 2);
		}

		public void determines_field_names_from_header_row(){
			var csv = CsvDataReader.Parse("Id,Value");
			csv.ReadHeader();

			Check.That(
				() => csv.GetName(0) == "Id",
				() => csv.GetName(1) == "Value");
		}

		public void throws_InvalidOperationExcetion_if_ReadHeader_or_SetFieldCount_not_called_before_FieldCount_is_used() {
			var csv = CsvDataReader.Parse("Id,Value");
			Check.Exception<InvalidOperationException>(() => Ignore(csv.FieldCount));
		}

		void Ignore<T>(T ignored) { }

		public void field_ordinals_match_header_ordering(){
			var csv = CsvDataReader.Parse("Id,Value");
			csv.ReadHeader();

			Check.That(
				() => csv.GetOrdinal("Id") == 0,
				() => csv.GetOrdinal("Value") == 1);
		}

		public void handle_escaped_fields() {
			var csv = CsvDataReader.Parse("\",\\\"");
			csv.ReadHeader();

			Check.That(() => csv.GetName(0) == ",\"");
		}

		public void field_ordinals_give_predecence_to_case_sensative_matches(){
			var csv = CsvDataReader.Parse("id,Id");
			csv.ReadHeader();
			Check.That(() => csv.GetOrdinal("Id") == 1);
		}

		public void field_ordinals_support_case_ignorant_matchin(){
			var csv = CsvDataReader.Parse("id");
			csv.ReadHeader();
			Check.That(() => csv.GetOrdinal("Id") == 0);
		}

		public void unknown_fields_throw_exception_when_getting_ordinal(){
			var csv = CsvDataReader.Parse("id");
			csv.ReadHeader();
			Check.Exception<IndexOutOfRangeException>(() => csv.GetOrdinal("MissingField"));
		}

		public void should_support_different_separators(){
			var csv = CsvDataReader.Parse("1;2", ';');
			csv.Read();

			Check.That(
				() => csv.GetValue(0).Equals("1"),
				() => csv.GetValue(1).Equals("2"));
		}

		public void supports_checking_if_field_exists(){
			var csv = CsvDataReader.Parse("First,Second");
			csv.ReadHeader();

			Check.That(
				() => csv.HasField("First"),
				() => csv.HasField("NotAvailable") == false);
		}

		public void field_existance_check_should_be_case_insensitive(){
			var csv = CsvDataReader.Parse("First");
			csv.ReadHeader();
			Check.That(() => csv.HasField("fiRst"));
		}

		public void should_support_multiple_data_rows() {
			var csv = CsvDataReader.Parse("First\r\n1\n2");
			csv.ReadHeader();
			IDataRecord record = csv;
			csv.Read();
			Check.That(() => record["First"].Equals("1"));
			csv.Read();
			Check.That(() => record["First"].Equals("2"));
		}

		public void should_stop_when_out_of_data() {
			var csv = CsvDataReader.Parse("First\r\n1\n2");
			csv.ReadHeader();

			Check.That(() => csv.Read());
			Check.That(() => csv.Read());
			Check.That(() => csv.Read() == false);
		}
	}
}