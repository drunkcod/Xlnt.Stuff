using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using NUnit.Framework;
using Xlnt.Stuff;

namespace Xlnt.Tests.Data
{
    class CsvReport<T>
    {
        struct CsvReportColumn
        {
            public string Name;
            public Func<T, string> GetValue;
        }

        const string FieldDelimiter = ";";
        static string RecordDelimiter { get { return Environment.NewLine; } }

            readonly TextWriter target;
        readonly List<CsvReportColumn> columns = new List<CsvReportColumn>();
        private string delimiter;

        public CsvReport(TextWriter target){
            this.target = target;
        }

        public bool WriteHeader { get; set; }

        public CsvReport<T> Map<TAny>(Expression<Func<T,TAny>> column){
            var get = column.Compile();
            
            columns.Add(new CsvReportColumn{
                Name = GetName(column),
                GetValue = x => get(x).ToString()
            });
            return this;
        }

        public void WriteAll(IEnumerable<T> collection){
            WriteHeaders();
            collection.Each(WriteRecord);
        }

        void WriteHeaders(){
            if(!WriteHeader)
                return;
            WriteColumns(x => x.Name);
        }

        void WriteRecord(T item){
            WriteColumns(x => x.GetValue(item));
        }

        void WriteColumns(Func<CsvReportColumn,string> getValue){
            for (var i = 0; i != columns.Count; ++i, delimiter = FieldDelimiter)
                target.Write("{0}{1}", delimiter, getValue(columns[i]));
            NextRecord();            
        }

        void NextRecord() { delimiter = RecordDelimiter; }

        static string GetName<TAny>(Expression<Func<T,TAny>> column){
            return ((MemberExpression)column.Body).Member.Name;
        }
    }

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
