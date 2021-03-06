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
        const string FieldDelimiter = ",";
        static string RecordDelimiter { get { return Environment.NewLine; } }

        readonly TextWriter target;
        readonly FieldCollection<T> columns = new FieldCollection<T>();
        private string delimiter;

        public CsvReport(TextWriter target){
            this.target = target;
        }

        public bool WriteHeader { get; set; }
        public FieldCollection<T> ColumnMappings { get { return columns; } }

        public void WriteAll(IEnumerable<T> collection){
            if (WriteHeader)
                WriteHeaders();
            collection.ForEach(WriteRecord);
        }

        public void WriteHeaders() {
            WriteColumns((name, read) => name);
        }

        public void WriteRecord(T item) {
            WriteColumns((name, read) => Sanitize(read(item).ToString()));
        }

        void WriteColumns(Func<string,Func<T,object>,string> getValue){
            for (var i = 0; i != columns.Count; ++i, delimiter = FieldDelimiter)
                target.Write("{0}{1}", delimiter, getValue(columns.GetName(i), columns.GetReader(i)));
            NextRecord();            
        }

        void NextRecord() { delimiter = RecordDelimiter; }

        static string Sanitize(string s){
            if(s.IndexOfAny(new []{ FieldDelimiter[0], '\n', '"'}) != -1)
                return string.Format("\"{0}\"",  s.Replace("\"", "\"\""));
            return s;
        }
    }
}