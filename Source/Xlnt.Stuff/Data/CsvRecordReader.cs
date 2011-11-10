using System;
using System.Collections.Generic;
using System.IO;

namespace Xlnt.Data
{
    public class CsvRecordReader : IDisposable
    {
        const int MaxFieldLength = 4096;
        const int MinChunkSize = MaxFieldLength;

        readonly TextReader reader;
        readonly char separator;
        readonly char[] buffer = new char[MaxFieldLength + MinChunkSize];
        int first, read , write, last;

        public CsvRecordReader(TextReader reader, char separator) {
            this.reader = reader;
            this.separator = separator;
        }

        public void Dispose() {
            reader.Dispose();
        }

        public char Separator { get { return separator; } }

        public void ReadRecord(Action<string> fieldReady) {
            if (fieldReady == null) throw new ArgumentNullException("fieldReady");
            for (; ; ) {
                var c = ReadNextChar();
                if (c == Separator)
                    fieldReady(StartNext());
                else switch (c) {
                    default: Store(); break;
                    case '"': ReadEscaped(); break;
                    case '\r': continue;
                    case '\n':
                        fieldReady(StartNext()); 
                        return;
                    case -1:
                        if (FieldReady)
                            goto case '\n';
                        return;
                }
            }
        }

        int AvailableChunkSpace { get { return buffer.Length - write; } }
        int FieldLength { get { return write - first; } }
        bool FieldReady { get { return write != first; } }

        void ReadEscaped() {
            for(int c; (c = ReadNextChar()) != -1; Store())
                if (c == '\\') {
                    if (ReadNextChar() == -1)
                        return;
                } else if (c == '"')
                    return;
        }

        int ReadNextChar() {
            if (OutOfData())
                return -1;
            return buffer[write] = buffer[read++];
        }

        bool OutOfData() {
            if (read != last)
                return false;
            if (AvailableChunkSpace < MinChunkSize) 
                RealignBuffer();
            var count = reader.Read(buffer, write, AvailableChunkSpace);
            if (count == 0)
                return true;
            last = write + count;
            return false;
        }

        void RealignBuffer() {
            read = write = FieldLength;
            Array.Copy(buffer, first, buffer, 0, write);
            first = 0;
        }
        
        void Store() { ++write; }

        string StartNext() {
            var field = new string(buffer, first, FieldLength);
            first = write;
            return field;
        }
    }
}