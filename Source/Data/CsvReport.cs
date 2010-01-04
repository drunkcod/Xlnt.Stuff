using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    /// <summary>
    /// RFC-4180 compatbile CsvReport.
    /// see: http://tools.ietf.org/html/rfc4180
    /// </summary>
    public class CsvReport<T>
    {
        struct CsvReportColumn
        {
            public string Name;
            public Func<T, string> GetValue;
        }

        const string FieldDelimiter = ",";
        static string RecordDelimiter { get { return Environment.NewLine; } }

        readonly TextWriter target;
        readonly List<CsvReportColumn> columns = new List<CsvReportColumn>();
        private string delimiter;

        public CsvReport(TextWriter target){
            this.target = target;
        }

        public bool WriteHeader { get; set; }

        public CsvReport<T> Map<TAny>(Expression<Func<T,TAny>> column){
            return Map(GetName(column), column.Compile());
        }

        public CsvReport<T> Map<TAny>(string name, Func<T,TAny> column)
        {
            columns.Add(new CsvReportColumn {
                Name = name,
                GetValue = x => column(x).ToString()
            });
            return this;
        }

        public void WriteAll(IEnumerable<T> collection){
            WriteHeaders();
            collection.Each(WriteRecord);
        }

        void WriteHeaders(){
            if(WriteHeader)
                WriteColumns(x => x.Name);
        }

        void WriteRecord(T item){
            WriteColumns(x => Sanitize(x.GetValue(item)));
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

        static string Sanitize(string s){
            if(s.IndexOfAny(new []{ FieldDelimiter[0], '\n', '"'}) != -1)
                return string.Format("\"{0}\"",  s.Replace("\"", "\"\""));
            return s;
        }
    }
}