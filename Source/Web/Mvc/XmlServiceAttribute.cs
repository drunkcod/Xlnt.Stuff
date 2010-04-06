using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Xlnt.Web.Mvc
{
    public class XmlServiceAttribute : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var request = filterContext.RequestContext.HttpContext.Request;
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null || UnsupportedAcceptType(request.AcceptTypes))
                return;
            filterContext.Result = new XmlResult(viewResult.ViewData.Model);
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) { }

        static bool UnsupportedAcceptType(IEnumerable<string> acceptTypes) {
            if (acceptTypes == null)
                return true;
            return !acceptTypes.Any(item => XmlResult.ContentType.Equals(item));
        }
    }
}
