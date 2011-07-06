using Cone;

namespace Xlnt.Data
{
    [Feature("Linq query rewriting")]
    public class LinqQueryRewriting
    {
        [Row("[MyTable] AS [t0]", "[MyTable] AS [t0] with(nolock)")
        ,Row("[t0].[MyTable] AS [t0]", "[t0].[MyTable] AS [t0]")//avoid Table/Column collision
        ,DisplayAs("{0} -> {1}")]
        public void nolock(string scope, string input, string expected) {
            var session = new LinqQueryRewritingSession();
            session.PushNoLockScope(new[]{ "MyTable" });
            Verify.That(() => session.Rewrite(input) == expected);
        }
    }
}
