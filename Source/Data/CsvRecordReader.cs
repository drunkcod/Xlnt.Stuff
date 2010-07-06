using System;
using System.Collections.Generic;
using System.IO;

namespace Xlnt.Data
{
    class CsvRecordReader : IDisposable
    {
        const int MaxFieldLength = 1024;
        const int MinChunkSize = 256;

        readonly TextReader reader;
        readonly char separator;
        readonly char[] buffer = new char[MaxFieldLength + MinChunkSize];
        int start = 0, read = 0, write = 0, lastChar = 0;
        char prev, curr = default(char);

        public CsvRecordReader(TextReader reader, char separator) {
            this.reader = reader;
            this.separator = separator;
        }

        public void Dispose() {
            reader.Dispose();
        }

        public char Separator { get { return separator; } }

        public string[] Read() {
            var items = new List<string>();
            ReadRecord(() => items.Add(new string(buffer, start, FieldLength)));
            return items.ToArray();
        }

        void ReadRecord(Action onField) {
            while (ReadNextChar()) {
                if (curr == Separator) {
                    onField();
                    start = write;
                }
                else if (curr == '\r')
                    continue;
                else if (curr == '\n')
                    break;
                else if (curr == '"')
                    ReadEscaped();
                else
                    Store(curr);
            }
            if (write != 0)
                onField();
        }

        int AvailableChunkSpace { get { return buffer.Length - write; } }
        int FieldLength { get { return write - start; } }

        bool ReadNextChar() {
            if (read == lastChar) {
                if (AvailableChunkSpace < MinChunkSize) {
                    read = write = FieldLength;
                    Array.Copy(buffer, start, buffer, 0, write);
                    start = 0;
                }
                var count = reader.Read(buffer, write, AvailableChunkSpace);
                if (count == 0)
                    return false;
                lastChar = write + count;
            }
            prev = curr;
            curr = buffer[read++];
            return true;
        }

        void ReadEscaped() {
            while (ReadNextChar()) {
                if (curr == '\\')
                    continue;
                if (curr == '"' && prev != '\\')
                    return;
                Store(curr);
            }
        }

        void Store(char ch) { buffer[write++] = ch; }
    }
}
