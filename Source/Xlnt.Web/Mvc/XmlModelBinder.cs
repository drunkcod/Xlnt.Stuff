using System;
using System.Web;
using System.Web.Mvc;

namespace Xlnt.Web.Mvc
{
    public class XmlModelBinder : IModelBinder, IModelBinderProvider
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            return XmlResult.Deserialize(controllerContext.HttpContext.Request.InputStream, bindingContext.ModelType);
        }

        public IModelBinder GetBinder(Type modelType) {
            var request = HttpContext.Current.Request;
            if (XmlResult.SupportsContentType(request.ContentType))
                return this;
            return null;
        }
    }
}
