using System;
using System.IO;
using Xlnt.IO;
using Xlnt.Stuff;
using System.Collections.Generic;

namespace Xlnt.Data
{
    public class CsvDataReader : DataReaderBase
    {
        static char DefaultSeparator = ',';

        readonly CsvRecordReader reader;
        List<string> values = new List<string>();
        string[] fields;

        public CsvDataReader(TextReader reader) : this(reader, DefaultSeparator) { }

        public CsvDataReader(TextReader reader, char separator) {
            this.reader = new CsvRecordReader(reader, separator);
        }

        public static CsvDataReader Parse(string s) { return new CsvDataReader(new StringReader(s)); }
        public static CsvDataReader Parse(string s, char separator) { return new CsvDataReader(new StringReader(s), separator); }

        public override int FieldCount { 
            get {
                if (fields == null) throw new InvalidOperationException("Must call ReadHeader or SetFieldCount before accessing fields by name.");
                return fields.Length; 
            } 
        }
        
        public char Separator { get { return reader.Separator; } }

        public void SetFieldCount(int count) {
            fields = new string[count];
        }
        
        public void ReadHeader() {
            ReadRecord();
            fields = values.ToArray();
        }

        public override bool Read() {
            ReadRecord();
            return values.Count > 0;
        }

        public override string GetName(int i) {
            return fields[i];
        }

        public bool HasField(string name) {
            return fields.Any(item => string.Compare(item, name, true) == 0);
        }

        public override object GetValue(int i){ return values[i]; }
        protected override void DisposeCore() { reader.Dispose(); }
		public override bool IsDBNull(int i) { return false; }

        void ReadRecord() {
            values.Clear();
            reader.ReadRecord(x => values.Add(x));
        }
    }
}