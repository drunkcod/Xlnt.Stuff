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

        readonly RecordReader reader;
        string[] values;
        string[] fields;

        public CsvDataReader(TextReader reader) : this(reader, DefaultSeparator) { }

        public CsvDataReader(TextReader reader, char separator) {
            this.reader = new RecordReader(reader, separator);
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

        public void SetFieldCount(int count) 
        {
            fields = new string[count];
        }
        
        public void ReadHeader(){
            fields = ReadRecord();
        }

        public override bool Read(){
            values = ReadRecord();
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

        string[] ReadRecord(){
            return reader.Read();
        }

        class RecordReader : IDisposable
        {
            const int MaxFieldLength = 1024;
            const int MinChunkSize = 256;

            readonly TextReader reader;
            readonly char separator;
            readonly char[] buffer = new char[MaxFieldLength + MinChunkSize];
            int start = 0, offset = 0, itemsAvailable = 0;

            public RecordReader(TextReader reader, char separator) {
                this.reader = reader;
                this.separator = separator;
            }

            public void Dispose() {
                reader.Dispose();
            }

            public char Separator { get { return separator; } }

            public string[] Read() {
                var items = new List<string>();
                ReadRecord(x => items.Add(new string(x.Array, x.Offset, x.Count)));
                return items.ToArray();
            }

            void ReadRecord(Action<ArraySegment<char>> onField) {
                char prev, curr = default(char);
                bool inEscaped = false;
                for (; ; )
                {
                    prev = curr;
                    var next = NextChar();
                    if (next == -1)
                        break;
                    curr = (char)next;
                    if (inEscaped)
                    {
                        if (curr == '"')
                            if (prev != '\\')
                                inEscaped = false;
                            else
                            {
                                offset -= 1;
                            }
                    }
                    else
                    {
                        if (curr == '\r')
                            continue;
                        if (curr == '\n')
                            break;
                        else if (curr == Separator)
                        {
                            onField(new ArraySegment<char>(buffer, start, offset - start));
                            offset = start = offset + 1;
                            continue;
                        }
                        else if (curr == '"')
                        {
                            inEscaped = true;
                            start += 1;
                        }
                    }
                    Store(curr);
                }
                if (offset != 0)
                    onField(new ArraySegment<char>(buffer, start, offset - start));
            }

            int NextChar() {
                if (itemsAvailable == 0) {
                    var chunkSize = buffer.Length - offset;
                    if (chunkSize < MinChunkSize) {
                        offset = offset - start;
                        Array.Copy(buffer, start, buffer, 0, offset);
                        start = 0;
                        chunkSize = buffer.Length - offset;
                    }
                    itemsAvailable = reader.Read(buffer, offset, chunkSize);
                    if (itemsAvailable == 0)
                        return -1;
                }
                --itemsAvailable;
                return buffer[offset];
            }
            void Store(char ch) { buffer[offset++] = ch; }
        }
    }
}