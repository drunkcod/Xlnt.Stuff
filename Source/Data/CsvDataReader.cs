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
            int start = 0, read = 0, write = 0, itemsAvailable = 0;
            char prev, curr = default(char);

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
                for (; ; )
                {
                    if (!ReadNextChar())
                        break;
                    if (curr == '\r')
                        continue;
                    if (curr == '\n')
                        break;
                    if (curr == Separator) {
                        onField(new ArraySegment<char>(buffer, start, FieldLength));
                        read = write = start = write + 1;
                        continue;
                    }
                    if (curr == '"') {
                        ReadEscaped();
                        continue;
                    }
                    Store(curr);
                }
                if (write != 0)
                    onField(new ArraySegment<char>(buffer, start, FieldLength));
            }

            int AvailableChunkSpace { get { return buffer.Length - write; } }
            int FieldLength { get { return write - start; } }

            bool ReadNextChar() {
                prev = curr;
                if (itemsAvailable == 0) {
                    if (AvailableChunkSpace < MinChunkSize) {
                        write = write - start;
                        Array.Copy(buffer, start, buffer, 0, write);
                        start = 0;
                    }
                    itemsAvailable = reader.Read(buffer, write, AvailableChunkSpace);
                    if (itemsAvailable == 0)
                        return false;
                }
                --itemsAvailable;
                curr = buffer[read++];
                return true;
            }

            void ReadEscaped() {
                while (ReadNextChar())
                {
                    if (curr == '"')
                        if (prev != '\\')
                            return;
                        else
                            --write;
                    Store(curr);
                }
            }

            void Store(char ch) { buffer[write++] = ch; }
        }
    }
}