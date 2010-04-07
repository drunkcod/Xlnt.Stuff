using System.Web.Mvc;

namespace Xlnt.Web.Mvc
{
    class XmlModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            var request = controllerContext.RequestContext.HttpContext.Request;
            if (!XmlResult.SupportsContentType(request.ContentType))
                return null;
            return XmlResult.Deserialize(request.InputStream, bindingContext.ModelType);
        }
    }
}
