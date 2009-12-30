using System;
using System.IO;

namespace Xlnt.IO
{
    class LineReader : ILineReader
    {
        readonly TextReader reader;
        public LineReader(TextReader reader) {
            this.reader = reader;
        }

        public string ReadLine() { return reader.ReadLine(); }
        public void Dispose() { reader.Dispose(); }
    }
}
