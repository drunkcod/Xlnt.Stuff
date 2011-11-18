using System;
using System.Web;
using System.Web.Mvc;
using System.Net.Mime;

namespace Xlnt.Web.Mvc
{
    public class XmlModelBinder : IModelBinder, IModelBinderProvider
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            return XmlResult.Deserialize(controllerContext.HttpContext.Request.InputStream, bindingContext.ModelType);
        }

        public IModelBinder GetBinder(Type modelType) {
            var contentType = HttpContext.Current.Request.ContentType;
            
            if(!string.IsNullOrEmpty(contentType) && XmlResult.SupportsContentType(new ContentType(contentType)))
                return this;
            return null;
        }
    }
}
