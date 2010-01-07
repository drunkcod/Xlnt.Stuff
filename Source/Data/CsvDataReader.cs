using System.IO;
using Xlnt.IO;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public class CsvDataReader : DataReaderBase
    {
        static readonly char[] DefaultSeparator = new[]{','};

        readonly ILineReader reader;
        string[] values;
        string[] fields;
        char[] separator = DefaultSeparator;
        
        public CsvDataReader(TextReader reader): this(new LineReader(reader)){}

        public CsvDataReader(ILineReader reader){
            this.reader = reader;
        }

        public static CsvDataReader Parse(string s) { return new CsvDataReader(new StringReader(s)); }

        public override int FieldCount { get { return fields.Length; } }
        
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
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return new string[0];
            return line.Split(separator);
        }
    }
}