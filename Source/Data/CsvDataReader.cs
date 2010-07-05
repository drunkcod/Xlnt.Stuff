using System;
using System.IO;
using Xlnt.IO;
using Xlnt.Stuff;
using System.Collections.Generic;

namespace Xlnt.Data
{
    public class CsvDataReader : DataReaderBase
    {
        static readonly char[] DefaultSeparator = new[]{','};

        readonly TextReader reader;
        string[] values;
        string[] fields;
        char[] separator = DefaultSeparator;
        
        public CsvDataReader(TextReader reader) {
            this.reader = reader;
        }

        public static CsvDataReader Parse(string s) { return new CsvDataReader(new StringReader(s)); }

        public override int FieldCount { 
            get {
                if (fields == null) throw new InvalidOperationException("Must call ReadHeader or SetFieldCount before accessing fields by name.");
                return fields.Length; 
            } 
        }
        
        public char Separator
        {
            get { return separator[0]; }
            set { separator = new[] {value}; }
        }

        public void SetFieldCount(int count) 
        {
            fields = new string[count];
        }
        
        public void ReadHeader(){
            fields = ReadLine();
        }

        public override bool Read(){
            values = ReadLine();
            return values.Length > 0;
        }

        public override string GetName(int i){
            return fields[i];
        }

        public bool HasField(string name){
            return fields.Any(item => string.Compare(item, name, true) == 0);
        }

        public override object GetValue(int i){ return values[i]; }
        protected override void DisposeCore() { reader.Dispose(); }

        string[] ReadLine(){
            var buff = new char[MaxFieldLength];
            var offset = 0;
            var start = 0;
            var items = new List<string>();
            char prev, curr = default(char);
            var curb = new char[1];
            bool inEscaped = false;
            for (; ; ) {
                prev = curr;
                if (reader.Read(curb, 0, 1) == 0)
                    break;
                curr = curb[0];
                if (inEscaped) {
                    if (curr == '"')
                        if (prev != '\\')
                            inEscaped = false;
                        else {
                            offset -= 1;
                        }
                }
                else {
                    if (curr == '\r')
                        continue;
                    if (curr == '\n')
                        break;
                    else if (curr == Separator)
                    {
                        items.Add(new String(buff, start, offset - start));
                        offset = 0;
                        start = 1;
                    }
                    else if (curr == '"') {
                        inEscaped = true;
                        start += 1;
                    }
                }
                buff[offset++] = curr;
            }
            if (items.Count == 0 && offset == 0)
                return new string[0];
            items.Add(new String(buff, start, offset - start));
            return items.ToArray();
        }

        const int MaxFieldLength = 4096;
    }
}