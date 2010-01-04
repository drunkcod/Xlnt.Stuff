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

        [SetUp]
        public void Setup(){
            target = new StringWriter();
            report = new CsvReport<ReportLine>(target);
        }

        [Test]
        public void Map_columns_for_via_Expressions(){
            report.Map(x => x.Id)
                .Map(x => x.Value);

            report.WriteAll(new[] {new ReportLine {Id = 42, Value = "The Answer"}});

            Assert.That(target.ToString(), Is.EqualTo("42;The Answer"));
        }
        [Test]
        public void Map_columns_via_names_and_lambdas(){
            report.Map("Foo", x => x.Id)
                .Map("Bar", x => x.Value);

            report.WriteHeader = true;
            report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

            Assert.That(target.ToString(), Is.EqualTo("Foo;Bar\r\n42;The Answer"));            
        }
        [Test]
        public void Expression_columns_generate_header_named_after_their_fields_or_properties(){
            report.Map(x => x.Id)
                .Map(x => x.Value);

            report.WriteHeader = true;
            report.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

            Assert.That(target.ToString(), Is.StringStarting("Id;Value"));            
        }
        [Test]
        public void should_write_one_record_per_line(){
            report.Map(x => x.Id)
                .Map(x => x.Value);

            report.WriteAll(new[]{
                new ReportLine { Id = 1, Value = "First" },
                new ReportLine { Id = 2, Value = "Second"}});

            Assert.That(target.ToString(), Is.EqualTo("1;First\r\n2;Second"));
        }
    }
}