using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace Xlnt.Web.Mvc
{
    public class XmlResult : ActionResult
    {
        XmlSerializer serializer;
        object value;

        public static XmlResult From(object value) {
            var serializer = new XmlSerializer(value.GetType(), "");
            return new XmlResult {
                serializer = serializer,
                value = value
            };
        }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;
            response.ContentType = "text/xml";
            response.ContentEncoding = Encoding.UTF8;

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (var xml = XmlWriter.Create(response.OutputStream, new XmlWriterSettings {
                Indent = true,
                CloseOutput = true
            }))
                serializer.Serialize(xml, value, ns);
        }
    }
}
