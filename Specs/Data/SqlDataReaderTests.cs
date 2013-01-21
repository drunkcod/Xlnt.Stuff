using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;

namespace Xlnt.Data
{
	[Feature("SqlDataReader")]
    public class SqlDataReaderTests : SqlBulkCopyFixture
    {
        public void What_happens_when_calling_GetOrdinal_for_invalid_field() {
            WithConnection(db => {
                using(var command = db.CreateCommand()) {
                    command.CommandText = "select 1 as Id";
                    var reader = command.ExecuteReader();
                    Verify.Throws<IndexOutOfRangeException>.When(() => reader.GetOrdinal("MissingColumn"));
                }
            });
        }
    }
}
