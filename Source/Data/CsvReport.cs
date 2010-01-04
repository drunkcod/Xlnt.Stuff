using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public class CsvReport<T>
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
                GetValue = x => get(x).ToString()});
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
}