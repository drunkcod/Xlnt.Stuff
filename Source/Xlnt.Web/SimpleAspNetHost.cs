using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Serialization;
using Xlnt.Stuff;
using HeaderValue = System.Collections.Generic.KeyValuePair<string, string>;

namespace Xlnt.Web
{
    [Serializable]
    public class SimpleAspNetHostResult
    {
        public int StatusCode;
        public string StatusDescription;
        public IDictionary<string,string> Headers = new Dictionary<string, string>();
        public string Body;

        public override string ToString() {
            var result = new StringBuilder();
            result.AppendFormat("{0} {1}\n", StatusCode, StatusDescription);
            Headers.ForEach(x => result.AppendFormat("{0}: {1}\n", x.Key, x.Value));
            return result.AppendFormat("\n{0}\n", Body).ToString();
        }
    }

    class SimpleAspNetHostWorkerRequest : HttpWorkerRequest
    {
        public override void EndOfRequest() {
            FlushResponse(true);
            Response.Body = responseBody.GetStringBuilder().ToString();
        }

        public SimpleAspNetHostResult Response = new SimpleAspNetHostResult();

        public string Method;
        public string Url;
        public string QueryString;
        public byte[] Body;
        public IDictionary<string,string> Headers;
        readonly StringWriter responseBody = new StringWriter();

        public override void FlushResponse(bool finalFlush) {
            responseBody.Flush();
        }

        public override string GetHttpVerbName() {
            return Method;
        }

        public override string GetHttpVersion() {
            return "HTTP/1.1";
        }

        public override string GetLocalAddress() {
            throw new NotImplementedException();
        }

        public override int GetLocalPort() {
            throw new NotImplementedException();
        }

        public override string GetQueryString() {
            return QueryString;
        }

        public override string GetRawUrl() {
            if(string.IsNullOrEmpty(QueryString))
                return Url;
            return Url + "?" + QueryString;
        }

        public override string GetRemoteAddress() {
            return string.Empty;
        }

        public override int GetRemotePort() {
            return 0;
        }

        public override string GetUriPath() {
            return Url;
        }

        public override void SendKnownResponseHeader(int index, string value) {
            SendUnknownResponseHeader(GetKnownRequestHeaderName(index), value);
        }

        public override void SendUnknownResponseHeader(string name, string value) {
            Response.Headers.Add(name, value);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
            throw new NotImplementedException();
        }

        public override void SendResponseFromFile(string filename, long offset, long length) {
            throw new NotImplementedException();
        }

        public override void SendResponseFromMemory(byte[] data, int length) {
            responseBody.Write(Encoding.UTF8.GetString(data, 0, length));
        }

        public override void SendStatus(int statusCode, string statusDescription) {
            Response.StatusCode = statusCode;
            Response.StatusDescription = statusDescription; 
        }

        public override byte[] GetPreloadedEntityBody() {
            return Body;
        }

        public override string GetKnownRequestHeader(int index) {
            string value;
            if(Headers.TryGetValue(GetKnownRequestHeaderName(index), out value))
                return value;
            return null;
        }
    }

    public class HostedHttpApplication : HttpApplication
    {
        static internal HostedHttpApplication Instance;

        public override void Init() {
            Instance = this;
        }
    }

    public class SimpleAspNetHost : MarshalByRefObject
    {
        public static SimpleAspNetHost Create(string physicalPath) {
            return Create(physicalPath, "/");
        }

        public static SimpleAspNetHost Create(string physicalPath, string vdir) {
            return (SimpleAspNetHost)ApplicationHost.CreateApplicationHost(typeof(SimpleAspNetHost), vdir, physicalPath);
        }

        public SimpleAspNetHostResult ProcessRequest(string method, string url, HeaderValue[] headers, byte[] body) {
            var headerLookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            headers.ForEach(x => headerLookup.Add(x.Key, x.Value));
            var parts = url.Split('?');

            var worker = new SimpleAspNetHostWorkerRequest {
                Method = method,
                Url = parts[0],
                QueryString = parts.Length > 1 ? parts[1] : null,
                Headers = headerLookup,
                Body = body
            };
            
            HttpRuntime.ProcessRequest(worker);
            worker.EndOfRequest();
            return worker.Response;
        }

        public void Start() { this.Get("/"); }

        public void SetApplicationState(string key, object value) {
            WithHttpContext(x => x.Application[key] = value);
        }

        public object GetApplicationState(string key) {
            return WithHttpContext(x => x.Application[key]);
        }

        public object RemoteControl(string method, params object[] parameters) {
            var app = HostedHttpApplication.Instance;
            if(app == null)
                throw new InvalidOperationException(string.Format("Application instance not found. Make sure you subclass {1} & {0}.Start() has been called or a request has been processed.", GetType().Name, typeof(HostedHttpApplication).Name));
            var appType = app.GetType();
            return appType.GetMethod(method).Invoke(app, parameters);
        }

        T WithHttpContext<T>(Func<HttpContext, T> f) {
            return f(new HttpContext(new SimpleAspNetHostWorkerRequest { Url = "/" }));
        }
    }

    public class SimpleMessage
    {
        public readonly string ContentType;
        public readonly byte[] Data;

        SimpleMessage(string contentType, byte[] data) {
            this.ContentType = contentType;
            this.Data = data;
        }

        public static SimpleMessage Text(string value) {
            return new SimpleMessage("text/plain; charset=utf-8", Encoding.UTF8.GetBytes(value));
        }

        public static SimpleMessage Xml(object value) {
            var serializer = new XmlSerializer(value.GetType());
            var result = new MemoryStream();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(XmlTextWriter.Create(result, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }), value, ns);
            return new SimpleMessage("text/xml; charset=utf-8", result.ToArray());
        }
    }

    public static class SimpleAspNetHostExtensions 
    {
        static byte[] EmptyBody = new byte[0];
        static HeaderValue[] NoHeaders = new HeaderValue[0];

        public static SimpleAspNetHostResult Get(this SimpleAspNetHost self, string url) {
            return self.ProcessRequest("GET", url, NoHeaders, EmptyBody);
        }

        public static SimpleAspNetHostResult Put(this SimpleAspNetHost self, string url, SimpleMessage body) {
            return ProcessMessage(self, "PUT", url, body);
        }

        public static SimpleAspNetHostResult Post(this SimpleAspNetHost self, string url, SimpleMessage body) {
            return ProcessMessage(self, "POST", url, body);
        }

        static SimpleAspNetHostResult ProcessMessage(SimpleAspNetHost self, string method, string url, SimpleMessage body) {
            return self.ProcessRequest(method, url, new[] {
                new HeaderValue("Content-Type", body.ContentType),
                new HeaderValue("Content-Length", body.Data.Length.ToString())
            }, body.Data);
        }

    }
}
