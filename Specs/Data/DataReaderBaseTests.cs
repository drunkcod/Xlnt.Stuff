using System;
using Moq;
using System.Data;
using Cone;

namespace Xlnt.Data
{
    [Describe(typeof(DataReaderBase))]
    public class DataReaderBaseTests
    {
        public void Implements_named_indexer() {
            var reader = new Mock<DataReaderBase>();
            reader.SetupGet(x => x.FieldCount).Returns(1);
            reader.Setup(x => x.GetName(0)).Returns("Field");

            var value = (reader.Object as IDataRecord)["Field"];

            reader.Verify(x => x.GetValue(0));
        }
    }
}
