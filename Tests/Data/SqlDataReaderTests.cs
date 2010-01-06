using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Xlnt.Tests.Data
{
    public class SqlDataReaderTests : SqlBulkCopyFixture
    {
        [Test]
        public void What_happens_when_calling_GetOrdinal_for_invalid_field() {
            WithConnection(db => {
                using(var command = db.CreateCommand()) {
                    command.CommandText = "select 1 as Id";
                    var reader = command.ExecuteReader();
                    Assert.Throws(typeof(IndexOutOfRangeException), () => reader.GetOrdinal("MissingColumn"));
                }
            });
        }
    }
}
