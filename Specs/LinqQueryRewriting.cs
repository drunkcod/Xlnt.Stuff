using Cone;

namespace Xlnt.Data
{
	[Feature("Linq query rewriting")]
	public class LinqQueryRewriting
	{
		[Row("SELECT [Foo] AS [Foo]", "SELECT [Foo] AS [Foo]")
		,Row("SELECT [Foo] AS [Foo], [Bar] AS [Bar]", "SELECT [Foo] AS [Foo], [Bar] AS [Bar]")
		,Row("FROM [Foo] AS [t0]", "FROM [Foo] AS [t0] with(nolock)")
		,Row("FROM [dbo].[Foo] AS [t0]", "FROM [dbo].[Foo] AS [t0] with(nolock)")
		,Row("INNER JOIN [Foo] AS [t0]", "INNER JOIN [Foo] AS [t0] with(nolock)")
		,Row("CROSS JOIN [Foo] AS [t0]", "CROSS JOIN [Foo] AS [t0] with(nolock)")
		,Row("LEFT OUTER JOIN [Foo] AS [t0]", "LEFT OUTER JOIN [Foo] AS [t0] with(nolock)")
		,Row("FROM [Foo] AS [t0], [Bar] AS [t1]", "FROM [Foo] AS [t0] with(nolock), [Bar] AS [t1] with(nolock)")
		,Row("    FROM [dbo].[Respondent] AS [t0]","    FROM [dbo].[Respondent] AS [t0] with(nolock)", DisplayAs = "Handles leading white-space")
		,DisplayAs("{0} -> {1}")]
		public void nolock_all(string input, string expected)
		{
			var session = new LinqQueryRewritingSession();
			Assume.That(() => session.Nolock == false);
			session.Nolock = true;
			Check.That(() => session.Nolock == true);
			Check.That(() => session.Rewrite(input) == expected);
		}

		public void supports_recompile_query_hint() {
			var session = new LinqQueryRewritingSession();
			session.AddHint(QueryHint.Recompile);
			Check.That(() => session.Rewrite("SELECT *\r\nFROM [Foo] AS [t0]") == "SELECT *\r\nFROM [Foo] AS [t0]\r\noption(recompile)");
		}

		public void supports_traditional_all_caps_formatting()
		{
			var session = new LinqQueryRewritingSession();
			Assume.That(() => session.UseTraditionalFormatting == false);
			session.UseTraditionalFormatting = true;
			session.Nolock = true;
			session.AddHint(QueryHint.Recompile);
			Check.That(() => session.Rewrite("SELECT *\r\nFROM [Foo] AS [t0]") == "SELECT *\r\nFROM [Foo] AS [t0] WITH(NOLOCK)\r\nOPTION(RECOMPILE)");
		}
	}
}
