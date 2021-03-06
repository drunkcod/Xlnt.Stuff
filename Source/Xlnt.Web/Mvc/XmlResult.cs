﻿using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Mime;

namespace Xlnt.Web.Mvc
{
    public class XmlResult : ActionResult
    {
        class NoBomUTF8Encoding : UTF8Encoding
        {
            public override byte[] GetPreamble() {
                return new byte[0];
            }
        }

        static XmlWriterSettings XmlWriterSettings = new XmlWriterSettings {
            Indent = true,
            CloseOutput = true,
            Encoding = new NoBomUTF8Encoding()
        };

        static readonly string[] SupportedContentTypes = new[]{ "text/xml", "application/xml" };

        public static bool SupportsContentType(ContentType contentType)
        {
            for(int i = 0; i != SupportedContentTypes.Length; ++i)
                if(SupportedContentTypes[i].Equals(contentType.MediaType))
                    return true;
            return false;
        }

        readonly object value;

        public XmlResult(object value) { 
            this.value = value; 
        }

        public override void ExecuteResult(ControllerContext context) {
            var response = context.HttpContext.Response;
            response.ContentType = SupportedContentTypes[0];
            response.ContentEncoding = XmlWriterSettings.Encoding;
            Serialize(response.OutputStream, value);
        }

        public static void Serialize(Stream stream, object value) {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(value.GetType(), "");
            using (var xml = XmlWriter.Create(stream, XmlWriterSettings))
                serializer.Serialize(xml, value, ns);
        }

        public static object Deserialize(Stream stream, Type type) {
            var serializer = new XmlSerializer(type, "");
            return serializer.Deserialize(stream);
        }
    }
}
