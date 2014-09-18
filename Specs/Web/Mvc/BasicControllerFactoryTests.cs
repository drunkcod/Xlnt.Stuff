using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Cone;
using Moq;
using System;

namespace Xlnt.Web.Mvc
{
    namespace Dummies 
    {
        class DummyController : Controller { }
    }
    
    public interface IDisposableController : IDisposable, IController { }
    
    [Describe(typeof(BasicControllerFactory))]
    public class BasicControllerFactoryTests
    {
        static RequestContext EmptyContext() { return new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()); }
        BasicControllerFactory Factory;

        [BeforeEach]
        public void EstablishContext() {
            Factory = new BasicControllerFactory();
        }

        public void returns_MissingController_if_no_matching_controller_available() {
            Check.That(() => Factory.CreateController(EmptyContext(), "MissingController") is MissingController);
        }

        public void dispose_Disposable_controllers() {
            var controller = new Mock<IDisposableController>(MockBehavior.Strict);
            controller.Setup(x => x.Dispose());

            Factory.ReleaseController(controller.Object);

            controller.VerifyAll();
        }

        public void controller_names_are_case_insensitive() {
            Factory.Register(new[]{ typeof(Dummies.DummyController) });

            Check.That(() => Factory.CreateController(EmptyContext(), "dummy") is Dummies.DummyController);
        }

        [Context("controller registration")]
        public class ControllerRegistration 
        {
            public void register_with_Func() {
                var factory = new BasicControllerFactory();
                var controller = new MissingController();
                factory.RegisterController("Foo", () => controller);
            
                Check.That(() => object.ReferenceEquals(factory.CreateController(EmptyContext(), "Foo"), controller));
            }
        }
    }
}