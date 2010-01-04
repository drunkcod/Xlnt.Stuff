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

        [Test]
        public void Map_columns_for_generic_collection_via_Expressions(){
            var target = new StringWriter();
            var csv = new CsvReport<ReportLine>(target)
                .Map(x => x.Id)
                .Map(x => x.Value);
            csv.WriteAll(new[] {new ReportLine {Id = 42, Value = "The Answer"}});

            Assert.That(target.ToString(), Is.EqualTo("42;The Answer"));
        }
        [Test]
        public void Expression_columns_generate_header_named_after_their_fields_or_properties(){
            var target = new StringWriter();
            var csv = new CsvReport<ReportLine>(target)
                .Map(x => x.Id)
                .Map(x => x.Value);
            csv.WriteHeader = true;
            csv.WriteAll(new[] { new ReportLine { Id = 42, Value = "The Answer" } });

            Assert.That(target.ToString(), Is.StringStarting("Id;Value"));            
        }
        [Test]
        public void should_write_one_record_per_line(){
            var target = new StringWriter();
            var csv = new CsvReport<ReportLine>(target)
                .Map(x => x.Id)
                .Map(x => x.Value);
            csv.WriteAll(new[]{
                new ReportLine { Id = 1, Value = "First" },
                new ReportLine { Id = 2, Value = "Second"}});

            Assert.That(target.ToString(), Is.EqualTo("1;First\r\n2;Second"));
        }

    }
}
