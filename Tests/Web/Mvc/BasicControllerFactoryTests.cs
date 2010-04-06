using System.Web;
using System.Web.Routing;
using Moq;
using NUnit.Framework;

namespace Xlnt.Web.Mvc
{
    [TestFixture]
    public class BasicControllerFactoryTests
    {
        static RequestContext EmptyContext() { return new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()); }
        
        [Test]
        public void Should_return_MissingController_if_no_matching_controller_available() {
            var factory = new BasicControllerFactory();
            Assert.That(factory.CreateController(EmptyContext(), "MissingController"), Is.TypeOf(typeof(MissingController)));
        }

        [Test]
        public void Should_support_register_by_Func() {
            var factory = new BasicControllerFactory();
            var controller = new MissingController();
            factory.RegisterController("Foo", () => controller);
            
            Assert.That(factory.CreateController(EmptyContext(), "Foo"), Is.SameAs(controller));
        }
    }
}