using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace Xlnt.Web.Mvc
{
    public class XmlResult : ActionResult
    {
        static readonly string[] SupportedContentTypes = new[]{ "text/xml", "application/xml" };

        public static bool SupportsContentType(string contentType)
        {
            for(int i = 0; i != SupportedContentTypes.Length; ++i)
                if(SupportedContentTypes[i].Equals(contentType))
                    return true;
            return false;
        }

        object value;

        public XmlResult(object value) { this.value = value; }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;
            response.ContentType = SupportedContentTypes[0];
            response.ContentEncoding = Encoding.UTF8;

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(value.GetType(), "");
            using (var xml = XmlWriter.Create(response.OutputStream, new XmlWriterSettings {
                Indent = true,
                CloseOutput = true
            }))
                serializer.Serialize(xml, value, ns);
        }

        public static object Deserialize(Stream stream, Type type) {
            var serializer = new XmlSerializer(type, "");
            return serializer.Deserialize(stream);
        }
    }
}
