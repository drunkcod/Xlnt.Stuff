using System.Web;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Cone;

namespace Xlnt.Web.Mvc
{
    [Describe(typeof(BasicControllerFactory))]
    public class BasicControllerFactoryTests
    {
        static RequestContext EmptyContext() { return new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()); }
        
        public void returns_MissingController_if_no_matching_controller_available() {
            var factory = new BasicControllerFactory();
            Assert.That(factory.CreateController(EmptyContext(), "MissingController"), Is.TypeOf(typeof(MissingController)));
        }

        [Context("controller registration")]
        public class ControllerRegistration 
        {
            public void register_with_Func() {
                var factory = new BasicControllerFactory();
                var controller = new MissingController();
                factory.RegisterController("Foo", () => controller);
            
                Assert.That(factory.CreateController(EmptyContext(), "Foo"), Is.SameAs(controller));
            }
        }
    }
}