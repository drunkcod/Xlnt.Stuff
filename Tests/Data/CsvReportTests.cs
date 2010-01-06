using System.IO;
using NUnit.Framework;
using Xlnt.Data;

namespace Xlnt.Tests.Data
{
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

        [SetUp]
        public void Setup(){
            target = new StringWriter();
            report = new CsvReport<ReportLine>(target);
        }

        [Test]
        public void Map_columns_for_via_Expressions(){
            report.ColumnMappings
                .Add(x => x.Id)
                .Add(x => x.Value);

            report.WriteAll(new[] {new ReportLine {Id = 42, Value = "The Answer"}});

            Assert.That(target.ToString(), Is.EqualTo("42,The Answer"));
        }
        [Test]
        public void Map_columns_via_names_and_lambdas(){
            report.ColumnMappings
                .Add("Foo", x => x.Id)
                .Add("Bar", x => x.Value);

            report.WriteHeader = true;
            report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

            Assert.That(target.ToString(), Is.EqualTo("Foo,Bar\r\n42,The Answer"));            
        }
        [Test]
        public void Expression_columns_generate_header_named_after_their_fields_or_properties(){
            report.ColumnMappings
                .Add(x => x.Id)
                .Add(x => x.Value);

            report.WriteHeader = true;
            report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

            Assert.That(target.ToString(), Is.StringStarting("Id,Value"));            
        }
        [Test]
        public void should_write_one_record_per_line(){
            report.ColumnMappings
                .Add(x => x.Id)
                .Add(x => x.Value);

            report.WriteAll(new[]{
                new ReportLine { Id = 1, Value = "First" },
                new ReportLine { Id = 2, Value = "Second"}});

            Assert.That(target.ToString(), Is.EqualTo("1,First\r\n2,Second"));
        }
        [Test]
        public void should_quote_field_delimiter() {
            report.ColumnMappings.Add(x => x.Value);
            report.WriteAll(new[]{ new ReportLine { Value = "," } });

            Assert.That(Result, Is.EqualTo("\",\""));
        }
        [Test]
        public void should_quote_line_breaks() {
            report.ColumnMappings.Add(x => x.Value);
            report.WriteAll(new[] { new ReportLine { Value = "\r\n" } });

            Assert.That(Result, Is.EqualTo("\"\r\n\""));
        }
        [Test]//Yes I know this sounds totally strange...
        public void should_double_quote_and_quote_quotes() {
            report.ColumnMappings.Add(x => x.Value);
            report.WriteAll(new[] { new ReportLine { Value = "\"" } });
            Assert.That(Result, Is.EqualTo("\"\"\"\""));
        }
    }
}