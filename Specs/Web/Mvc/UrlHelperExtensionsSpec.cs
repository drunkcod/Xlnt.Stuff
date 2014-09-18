using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Cone;
using Moq;

namespace Xlnt.Web.Mvc
{
    [Describe(typeof(UrlHelperExtensions))]
    public class UrlHelperExtensionsSpec
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
            Check.That(() => url.Action(() => Index(42)) == "/Foo/42/Index");
        }

        public void multi_argument_action() {
            var url = UrlHelperFor("Multiple");
            Check.That(() => url.Action(() => Foo(7, "Moar")) == "/Multiple/7/Foo/Moar");
        }

        public void AbsoluteUrl_included_scheme_and_authority() {
            var url = UrlHelperFor("Foo");
            Check.That(() => url.Absolute("/") == "http://localhost/");
        }

        public static int DefaultId = 3;

        public void supports_reading_static_fields() {
            var url = UrlHelperFor("Foo");
            Check.That(() => url.Action(() => Index(DefaultId)) == "/Foo/" + DefaultId + "/Index");
        }

        string Indirect<T>(T value, Func<T, string> block) { return block(value); }

        void Index(int id) { }
        void Foo(int id, string extra) { }

        IUrlFactory UrlHelperFor(string controller) {
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
            return new UrlHelper(new RequestContext(httpContext.Object, routeData), RouteTable).AsUrlFactory();
        }
    }
}
