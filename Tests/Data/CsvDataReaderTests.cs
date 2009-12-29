using System.IO;
using NUnit.Framework;
using Xlnt.Data;

namespace Xlnt.Tests.Data
{
    public class CsvDataReaderTests
    {
        [Test]
        public void cant_Read_from_empty_stream(){
            var csv = new CsvDataReader(new StringReader(string.Empty));
            Assert.That(csv.Read(), Is.False);
        }

        [Test]
        public void GetValue_returns_fields_in_order(){
            var csv = new CsvDataReader(new StringReader("1,2,3"));
            csv.Read();

            Assert.That(new[]{ csv.GetValue(0), csv.GetValue(1), csv.GetValue(2)}, Is.EqualTo(new[]{"1", "2", "3"}));            
        }
    }
}