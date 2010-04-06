using System.Web.Mvc;
using System.Web.Routing;

namespace Xlnt.Web.Mvc
{
    public class MissingController : IController
    {
        public void Execute(RequestContext requestContext) {
            requestContext.HttpContext.Response.StatusCode = 404;
        }
    }
}