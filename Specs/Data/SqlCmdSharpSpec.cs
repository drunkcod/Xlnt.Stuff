using Cone;

namespace Xlnt.Data
{
    [Describe(typeof(SqlCmdSharp))]
    public class SqlCmdSharpSpec
    {
        [Row("GO", true)
        ,Row("go", true)
        ,Row("GO 100", true)
        ,Row("go --comment", true)
        ,Row(" GO", true)
        ,Row("GOTO", false)]
        public void end_of_batch_separator(string line, bool ok) {
            Verify.That(() => SqlCmdSharp.IsEndOfBatch(line) == ok);
        }
    }
}
