using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace Xlnt.Data
{
    [TestFixture]
    public class DataReaderBaseTests
    {
        [Test]
        public void Implements_named_indexer() {
            var reader = new Mock<DataReaderBase>();
            reader.SetupGet(x => x.FieldCount).Returns(1);
            reader.Setup(x => x.GetName(0)).Returns("Field");

            var value = (reader.Object as IDataRecord)["Field"];

            reader.Verify(x => x.GetValue(0));
        }
    }
}
