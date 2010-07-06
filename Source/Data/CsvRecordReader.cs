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
        int first = 0, read = 0, write = 0, lastChar = 0;
        char prev, curr = default(char);

        public CsvRecordReader(TextReader reader, char separator) {
            this.reader = reader;
            this.separator = separator;
        }

        public void Dispose() {
            reader.Dispose();
        }

        public char Separator { get { return separator; } }

        public void ReadRecord(Action<string> fieldReady) {
            while (ReadNextChar()) {
                if (curr == Separator) {
                    fieldReady(CurrentField);
                    StartNext();
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
            if (FieldReady)
                fieldReady(CurrentField);
        }

        int AvailableChunkSpace { get { return buffer.Length - write; } }
        string CurrentField { get { return new string(buffer, first, FieldLength); } }
        int FieldLength { get { return write - first; } }
        bool FieldReady { get { return write != 0; } }

        bool ReadNextChar() {
            if (OutOfData())
                return false;
            prev = curr;
            curr = buffer[read++];
            return true;
        }

        bool OutOfData() {
            if (read != lastChar)
                return false;
            if (AvailableChunkSpace < MinChunkSize) 
                RealignBuffer();
            var count = reader.Read(buffer, write, AvailableChunkSpace);
            if (count == 0)
                return true;
            lastChar = write + count;
            return false;
        }

        void RealignBuffer() {
            read = write = FieldLength;
            Array.Copy(buffer, first, buffer, 0, write);
            first = 0;
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

        void StartNext() { first = write; }
    }
}
