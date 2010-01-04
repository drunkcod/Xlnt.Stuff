using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Xlnt.Tests.Data
{
    class CsvReport<T>
    {
        readonly TextWriter target;
        readonly List<Func<T,string>> columns= new List<Func<T, string>>();

        public CsvReport(TextWriter target){
            this.target = target;
        }

        public CsvReport<T> Map<TAny>(Expression<Func<T,TAny>> column){
            var get = column.Compile();
            columns.Add(x => get(x).ToString());
            return this;
        }

        public void WriteAll(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                WriteRecord(item);
        }

        void WriteRecord(T item)
        {
            var delimiter = "";
            for(var i = 0; i != columns.Count; ++i, delimiter = ";")
                target.Write("{0}{1}", delimiter, columns[i](item));
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
    }
}
