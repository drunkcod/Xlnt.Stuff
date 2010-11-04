using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Cone;

namespace Xlnt.Web.Mvc
{
    [Describe(typeof(UrlHelperExtensions))]
    public class UrlHelperExtensionsTests
    {
        RouteCollection RouteTable;

        [BeforeEach]
        public void InitializeRouteTable() {
            RouteTable = new RouteCollection();
            RouteTable.MapRoute("DefaultExtra", "{controller}/{id}/{action}/{extra}", new { controller = "Default", action = "", id = "" }); 
            RouteTable.MapRoute("Default", "{controller}/{id}/{action}", new { controller = "Default", action = "", id = "" });
        }        

        public void niladic_lambda_action() {
            var url = UrlHelperFor("Foo");            
            Assert.That(url.Action(() => Index(42)), Is.EqualTo("/Foo/42/Index"));
        }

        public void niladic_indirect_lambda_action() {
            var url = UrlHelperFor("Foo");
            Assert.That(Indirect(42, x => url.Action(() => Index(x))), Is.EqualTo("/Foo/42/Index"));
        }

        public void multi_argument_action() {
            var url = UrlHelperFor("Multiple");
            Assert.That(url.Action(() => Foo(7, "Moar")), Is.EqualTo("/Multiple/7/Foo/Moar"));
        }

        public void AbsoluteUrl_included_scheme_and_authority() {
            var url = UrlHelperFor("Foo");
            Assert.That(url.Absolute("/"), Is.EqualTo("http://localhost/"));
        }

        public static int DefaultId = 3;

        public void Should_handle_static_fields() {
            var url = UrlHelperFor("Foo");
            Assert.That(url.Action(() => Index(DefaultId)), Is.EqualTo("/Foo/" + DefaultId + "/Index"));
        }

        string Indirect<T>(T value, Func<T, string> block) { return block(value); }

        void Index(int id) { }
        void Foo(int id, string extra) { }

        UrlHelper UrlHelperFor(string controller) {
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            httpContext.SetupGet(x => x.Request).Returns(() => {
                var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
                request.SetupGet(x => x.ApplicationPath).Returns("/");
                request.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());
                request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/"));
                return request.Object;
            });
            var response = new Mock<HttpResponseBase>();
            httpContext.SetupGet(x => x.Response).Returns(() => {
                response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => x);
                return response.Object;
            });
            var routeData = new RouteData();
            routeData.Values.Add("controller", controller);
            return new UrlHelper(new RequestContext(httpContext.Object, routeData), RouteTable);
        }
    }
}
