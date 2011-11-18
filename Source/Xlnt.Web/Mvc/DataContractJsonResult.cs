using System.Text;
using System.Web.Mvc;
using System.Runtime.Serialization.Json;

namespace Xlnt.Web.Mvc
{
    public class DataContractJsonResult : ActionResult
    {
        readonly object value;

        public DataContractJsonResult(object value) {
            this.value = value;
        }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;
            response.ContentType = "text/json";
            response.ContentEncoding = Encoding.UTF8;
            var serializer = new DataContractJsonSerializer(value.GetType());
            serializer.WriteObject(response.OutputStream, value);
        }
    }
}
