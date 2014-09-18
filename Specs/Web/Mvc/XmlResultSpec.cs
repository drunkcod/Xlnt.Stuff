using System.IO;
using Cone;
using System.Xml;
using System.Text;

namespace Xlnt.Web.Mvc
{
    [Describe(typeof(XmlResult))]
    public class XmlResultSpec
    {
        public void omits_bom() {
            var result = new MemoryStream();
            XmlResult.Serialize(result, "Hello");
            Check.That(() => result.ToArray().Length == Encoding.UTF8.GetByteCount("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<string>Hello</string>"));
        }
    }
}
