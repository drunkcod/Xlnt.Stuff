using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Xlnt.Web
{
    [Serializable]
    public class SimpleHostResult
    {
        public int StatusCode;
        public string StatusDescription;
        public IDictionary<string,string> Headers = new Dictionary<string, string>();
        public string Body;

        public override string ToString() {
            var result = new StringBuilder();
            result.AppendFormat("{0} {1}\n", StatusCode, StatusDescription);
            if(Headers != null)
                foreach(var item in Headers)
                    result.AppendFormat("{0}: {1}\n", item.Key, item.Value);
            return result.AppendFormat("\n{0}\n", Body).ToString();
        }
    }

    class SimpleHostWorkerRequest : HttpWorkerRequest
    {
        public override void EndOfRequest() {
            FlushResponse(true);
            Response.Body = responseBody.GetStringBuilder().ToString();
        }

        public SimpleHostResult Response = new SimpleHostResult();

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
            throw new NotImplementedException();
        }

        public override int GetRemotePort() {
            throw new NotImplementedException();
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

    public class SimpleAspNetHost : MarshalByRefObject
    {
        public static SimpleAspNetHost Create(string physicalPath) {
            return (SimpleAspNetHost)ApplicationHost.CreateApplicationHost(typeof(SimpleAspNetHost), "/", physicalPath);
        }

        public SimpleHostResult Get(string url) {
            return ProcessRequest("GET", url, new Dictionary<string,string>(), null);
        }

        public SimpleHostResult ProcessRequest(string method, string url, Dictionary<string,string> headers, byte[] body) {
            var parts = url.Split('?');
            var worker = new SimpleHostWorkerRequest {
                Method = method,
                Url = parts[0],
                QueryString = parts.Length > 1 ? parts[1] : null,
                Headers = new Dictionary<string, string>(headers, StringComparer.InvariantCultureIgnoreCase),
                Body = body
            };
            HttpRuntime.ProcessRequest(worker);
            worker.EndOfRequest();
            return worker.Response;
        }
    }}
