using Cone;

namespace Xlnt.Data
{
    [Feature("Linq query rewriting")]
    public class LinqQueryRewriting
    {
        [Row("FROM [MyTable] AS [t0]", "FROM [MyTable] AS [t0] with(nolock)")
        ,Row("FROM [t0].[MyTable] AS [t0]", "FROM [t0].[MyTable] AS [t0]")//avoid Table/Column collision
        ,DisplayAs("{0} -> {1}")]
        public void nolock(string input, string expected) {
            var session = new LinqQueryRewritingSession();
            session.PushNoLockScope("MyTable");
            Verify.That(() => session.Rewrite(input) == expected);
        }

        [Row("dbo.MyTable", "FROM [dbo].[MyTable] AS [t0]", "FROM [dbo].[MyTable] AS [t0] with(nolock)")
        ,Row("dbo.[MyTable]", "FROM [dbo].[MyTable] AS [t0]", "FROM [dbo].[MyTable] AS [t0] with(nolock)")
        ,DisplayAs("{1} -> {2}")]
        public void nolock_with_owner_prefix(string table, string input, string expected) {
            var session = new LinqQueryRewritingSession();
            session.PushNoLockScope(table);
            Verify.That(() => session.Rewrite(input) == expected);
        }

		[Row("SELECT [Foo] AS [Foo]", "SELECT [Foo] AS [Foo]")
		,Row("FROM [Foo] AS [t0]", "FROM [Foo] AS [t0] with(nolock)")
		,Row("JOIN [Foo] AS [t0]", "JOIN [Foo] AS [t0] with(nolock)")
		,Row("FROM [Foo] AS [t0], [Bar] AS [t1]", "FROM [Foo] AS [t0] with(nolock), [Bar] AS [t1] with(nolock)")
		,DisplayAs("{0} -> {1}")]
		public void nolock_all(string input, string expected)
		{
            var session = new LinqQueryRewritingSession();
            session.PushNoLockAll();
            Verify.That(() => session.Rewrite(input) == expected);
		}

        public void handle_escaped_table_names() {
            var session = new LinqQueryRewritingSession();
            session.PushNoLockScope("[MyTable]");
            Verify.That(() => session.Rewrite("FROM [MyTable] AS [t0]") == "FROM [MyTable] AS [t0] with(nolock)");
        }

		public void supports_recompile_query_hint() {
            var session = new LinqQueryRewritingSession();
			session.AddHint(QueryHint.Recompile);
            Verify.That(() => session.Rewrite("select * from Foo") == "select * from Foo\noption(recompile)");
		}
    }
}
