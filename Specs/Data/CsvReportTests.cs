using System.IO;
using Cone;
using Xlnt.Data;

namespace Xlnt.Data
{
	[Describe(typeof(CsvReport<>))]
	public class CsvReportTests
	{
		class ReportLine
		{
			public int Id { get; set; }
			public string Value;
		}

		private TextWriter target;
		private CsvReport<ReportLine> report;

		string Result { get { return target.ToString(); } }

		[BeforeEach]
		public void Setup(){
			target = new StringWriter();
			report = new CsvReport<ReportLine>(target);
		}

		public void Map_columns_for_via_Expressions(){
			report.ColumnMappings
				.Add(x => x.Id)
				.Add(x => x.Value);

			report.WriteAll(new[] {new ReportLine {Id = 42, Value = "The Answer"}});

			Check.That(() => target.ToString() == "42,The Answer");
		}

		public void Map_columns_via_names_and_lambdas(){
			report.ColumnMappings
				.Add("Foo", x => x.Id)
				.Add("Bar", x => x.Value);

			report.WriteHeader = true;
			report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

			Check.That(() => target.ToString() == "Foo,Bar\r\n42,The Answer");            
		}

		public void Expression_columns_generate_header_named_after_their_fields_or_properties(){
			report.ColumnMappings
				.Add(x => x.Id)
				.Add(x => x.Value);

			report.WriteHeader = true;
			report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

			Check.That(() => target.ToString().StartsWith("Id,Value"));            
		}

		public void should_write_one_record_per_line(){
			report.ColumnMappings
				.Add(x => x.Id)
				.Add(x => x.Value);

			report.WriteAll(new[]{
				new ReportLine { Id = 1, Value = "First" },
				new ReportLine { Id = 2, Value = "Second"}});

			Check.That(() => target.ToString() == "1,First\r\n2,Second");
		}

		public void should_quote_field_delimiter() {
			report.ColumnMappings.Add(x => x.Value);
			report.WriteAll(new[]{ new ReportLine { Value = "," } });

			Check.That(() => Result == "\",\"");
		}

		public void should_quote_line_breaks() {
			report.ColumnMappings.Add(x => x.Value);
			report.WriteAll(new[] { new ReportLine { Value = "\r\n" } });

			Check.That(() => Result == "\"\r\n\"");
		}
		
		//Yes I know this sounds totally strange...
		public void should_double_quote_and_quote_quotes() {
			report.ColumnMappings.Add(x => x.Value);
			report.WriteAll(new[] { new ReportLine { Value = "\"" } });
			Check.That(() => Result == "\"\"\"\"");
		}
	}
}