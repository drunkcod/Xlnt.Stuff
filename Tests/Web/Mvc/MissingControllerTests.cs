using System.Web;
using System.Web.Routing;
using Cone;
using Moq;

namespace Xlnt.Web.Mvc
{
    [Describe(typeof(MissingController))]
    public class MissingControllerTests
    {
        public void sets_response_StatusCode_to_404() {
            var controller = new MissingController();
            var response = new Mock<HttpResponseBase>();
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(x => x.Response).Returns(response.Object);
            var request = new RequestContext(httpContext.Object, new RouteData());               
            controller.Execute(request);

            response.VerifySet(x => x.StatusCode = 404);
        }
    }
}
