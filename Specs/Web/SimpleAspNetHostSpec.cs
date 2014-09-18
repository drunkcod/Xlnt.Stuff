using System;
using System.IO;
using Cone;

namespace Xlnt.Web
{
	[Describe(typeof(SimpleAspNetHost))]
	public class SimpleAspNetHostSpec
	{
		SimpleAspNetHost SampleSite;

		[BeforeAll]
		public void CreateSampleSiteHost() {
			SampleSite = SimpleAspNetHost.Create(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath), "..\\Specs\\Xlnt.Web.SampleSite")));
		}

		[AfterAll]
		public void ShutdownHost() {
			SampleSite = null;
		}

		[Row(200)
		,Row(400)
		,Row(401)
		,Row(403)
		,Row(404)
		,Row(422)
		,DisplayAs("{0}", Heading = "StatusCode")]
		public void StatusCodeHandling(int statusCode) {
			Check.That(() => SampleSite.Get("/Echo?status=" + statusCode).StatusCode == statusCode);
		}
	}
}
