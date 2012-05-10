using System.Web.Mvc;

namespace Xlnt.Web.Mvc
{
    public class HttpUnprocessableEntityResult : ActionResult
    {
        const int StatusCode = 422;
        const string StatusDescription = "The request was well-formed but was unable to be followed due to semantic errors.";

        readonly ActionResult inner;

        public HttpUnprocessableEntityResult(ActionResult inner) {
            this.inner = inner;
        }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;

            response.StatusCode = StatusCode;
            response.StatusDescription = StatusDescription;

            inner.ExecuteResult(context);
        }
    }
}
