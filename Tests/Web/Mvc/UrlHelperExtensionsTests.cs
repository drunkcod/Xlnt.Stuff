using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using System.Web;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reflection;

namespace Xlnt.Web.Mvc
{
    [TestFixture]
    public class UrlHelperExtensionsTests
    {
        RouteCollection RouteTable;

        [SetUp]
        public void InitializeRouteTable() {
            RouteTable = new RouteCollection();
            RouteTable.MapRoute("DefaultExtra", "{controller}/{id}/{action}/{extra}", new { controller = "Default", action = "", id = "" }); 
            RouteTable.MapRoute("Default", "{controller}/{id}/{action}", new { controller = "Default", action = "", id = "" });
        }        

        [Test]
        public void Should_support_niladic_lambda_action() {
            var url = UrlHelperFor("Foo");            
            Assert.That(url.Action(() => Index(42)), Is.EqualTo("/Foo/42/Index"));
        }
        [Test]
        public void Should_support_niladic_indirect_lambda_action() {
            var url = UrlHelperFor("Foo");
            Assert.That(Indirect(42, x => url.Action(() => Index(x))), Is.EqualTo("/Foo/42/Index"));
        }
        [Test]
        public void Should_support_multi_argument_action() {
            var url = UrlHelperFor("Multiple");
            Assert.That(url.Action(() => Foo(7, "Moar")), Is.EqualTo("/Multiple/7/Foo/Moar"));
        }
        [Test]
        public void AbsoluteUrl() {
            var url = UrlHelperFor("Foo");
            Assert.That(url.Absolute("/"), Is.EqualTo("http://localhost/"));
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
            var data = new RouteData();
            data.Values.Add("controller", controller);
            var requestContext = new Mock<RequestContext>(MockBehavior.Strict, httpContext.Object, data);

            return new UrlHelper(requestContext.Object, RouteTable);
        }
    }
}
