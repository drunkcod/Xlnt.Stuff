using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Xlnt.Tests.Data
{
    class CsvReport<T>
    {
        struct CsvReportColumn
        {
            public string Name;
            public Func<T, string> GetValue;
        }
        readonly TextWriter target;
        readonly List<CsvReportColumn> columns = new List<CsvReportColumn>();

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
            foreach (var item in collection)
                WriteRecord(item);
        }

        void WriteHeaders(){
            if(!WriteHeader)
                return;
            var delimiter = "";
            for (var i = 0; i != columns.Count; ++i, delimiter = ";")
                target.Write("{0}{1}", delimiter, columns[i].Name);
        }

        void WriteRecord(T item){
            var delimiter = "";
            for(var i = 0; i != columns.Count; ++i, delimiter = ";")
                target.Write("{0}{1}", delimiter, columns[i].GetValue(item));
        }

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
    }
}
