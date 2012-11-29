using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Serialization;
using Xlnt.Stuff;

namespace Xlnt.Web
{
    static class HttpHeaders
    {
        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";
        public const string Location = "Location";
    }

	[Serializable]
	public struct HeaderValue
	{
		public HeaderValue(string name, string value) {
			Name = name;
			Value = value;
		}

		public readonly string Name;
		public readonly string Value;
	}

    [Serializable]
    public class SimpleAspNetHostResponse
    {
        public int StatusCode;
        public string StatusDescription;
		public TimeSpan Duration;
        
        public string ContentType { get { return HeaderOrDefault(HttpHeaders.ContentType, string.Empty); } }
        public string Location { get { return HeaderOrDefault(HttpHeaders.Location, string.Empty); } }

        public IDictionary<string,string> Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public string Body;

        public T BodyAs<T>() {
            if(ContentType.StartsWith("text/xml")) {
                using(var reader = new StringReader(Body)) {
                    var serializer = new XmlSerializer(typeof(T), "");
                    return (T)serializer.Deserialize(reader);
                }
            }

            throw new NotSupportedException(string.Format("Unsupported {0}: {1}", HttpHeaders.ContentType, ContentType));
        }

        public override string ToString() {
            var result = new StringBuilder();
            result.AppendFormat("{0} {1}\n", StatusCode, StatusDescription);
            Headers.ForEach(x => result.AppendFormat("{0}: {1}\n", x.Key, x.Value));
            return result.AppendFormat("\n{0}\n", Body).ToString();
        }

		string HeaderOrDefault(string headerName, string defaultValue) {
			string value;
			return Headers.TryGetValue(headerName, out value) ? value : defaultValue;
		}
    }

    class SimpleAspNetHostWorkerRequest : HttpWorkerRequest
    {
        public override void EndOfRequest() {
            FlushResponse(true);
            Response.Body = Encoding.UTF8.GetString(responseBody.ToArray());
        }

        public SimpleAspNetHostResponse Response = new SimpleAspNetHostResponse();

        public string Method;
        public string Url;
        public string QueryString;
        public byte[] Body;
        public IDictionary<string,string> Headers;

        readonly MemoryStream responseBody = new MemoryStream();

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
            return IPAddress.Loopback.ToString();
        }

        public override int GetLocalPort() {
            return 80;
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
            SendUnknownResponseHeader(GetKnownResponseHeaderName(index), value);
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
            responseBody.Write(data, 0, length);
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
		class ApplicationProxy : MarshalByRefObject
		{
			public SimpleAspNetHostResponse ProcessRequest(string method, string url, HeaderValue[] headers, byte[] body) {
				var headerLookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				headers.ForEach(x => headerLookup.Add(x.Name, x.Value));
				var parts = url.Split('?');

				var worker = new SimpleAspNetHostWorkerRequest {
					Method = method,
					Url = parts[0],
					QueryString = parts.Length > 1 ? parts[1] : null,
					Headers = headerLookup,
					Body = body
				};
				var responseDuration = Stopwatch.StartNew();
				HttpRuntime.ProcessRequest(worker);
				worker.Response.Duration = responseDuration.Elapsed;
				worker.EndOfRequest();

				return worker.Response;
			}

			public void SetApplicationState(string key, object value) {
				WithHttpContext(x => x.Application[key] = value);
			}

			public object GetApplicationState(string key) {
				return WithHttpContext(x => x.Application[key]);
			}

			public object Invoke(string method, params object[] parameters) {
				if(HostedHttpApplication.Instance == null)
					throw new InvalidOperationException(string.Format("Application instance not found. Make sure you subclass {1} & {0}.Start() has been called or a request has been processed.", GetType().Name, typeof(HostedHttpApplication).Name));
			
				var appType = HostedHttpApplication.Instance.GetType();
				var targetMethod = appType.GetMethod(method);
				return targetMethod.Invoke(HostedHttpApplication.Instance, parameters);
			}

			T WithHttpContext<T>(Func<HttpContext, T> f) {
				return f(new HttpContext(new SimpleAspNetHostWorkerRequest { Url = "/" }));
			}
		}

		readonly ApplicationProxy proxy;

		private SimpleAspNetHost(ApplicationProxy proxy) { this.proxy = proxy; }

        public static SimpleAspNetHost Create(string physicalPath) {
            return Create(physicalPath, "/");
        }

        public static SimpleAspNetHost Create(string physicalPath, string vdir) {
            return new SimpleAspNetHost((ApplicationProxy)ApplicationHost.CreateApplicationHost(typeof(ApplicationProxy), vdir, physicalPath));
        }

		public event EventHandler<SimpleAspNetHostEventArgs> RequestProcessed;

        public SimpleAspNetHostResponse ProcessRequest(string method, string url, HeaderValue[] headers, byte[] body) {
			var response = proxy.ProcessRequest(method, url, headers, body);
			RequestProcessed.Raise(this, new SimpleAspNetHostEventArgs {
				Method = method,
				Url = url,
				Response = response
			});
            return response;
        }

        public void SetApplicationState(string key, object value) { proxy.SetApplicationState(key, value); }

        public object GetApplicationState(string key) { return proxy.GetApplicationState(key); }

        public object Invoke(string method, params object[] parameters) {
			return proxy.Invoke(method, parameters);
		}
    }

	public class SimpleAspNetHostEventArgs : EventArgs
	{
		public string Method;
		public string Url;
		public SimpleAspNetHostResponse Response;
	}

	public interface IMessageBuilder
    {
		IMessageBuilder AddHeader(string header, string value);
        IMessageBuilder XmlBody(object value);
        IMessageBuilder TextBody(string value);
    }

	public static class MessageBuilderExtensions 
	{
		public static IMessageBuilder Accept(this IMessageBuilder self, string value) { return self.AddHeader("Accept", value); }
	}

    class SimpleMessageBuilder : IMessageBuilder 
    {
		static readonly byte[] NoData = new byte[0];
        internal byte[] Data = NoData;
		internal Dictionary<string, string> Headers = new Dictionary<string, string>();  

        public IMessageBuilder TextBody(string value) {
            AddHeader(HttpHeaders.ContentType, "text/plain; charset=utf-8");
            Data = Encoding.UTF8.GetBytes(value);
            return this;
        }

        public IMessageBuilder XmlBody(object value) {
            var serializer = new XmlSerializer(value.GetType());
            var result = new MemoryStream();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(XmlWriter.Create(result, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }), value, ns);
            AddHeader(HttpHeaders.ContentType, "text/xml; charset=utf-8");
            Data = result.ToArray();
            return this;
        }

		public IMessageBuilder AddHeader(string header, string value)
		{
			Headers.Add(header, value);
			return this;
		}
    }

	public class HttpHeaderCollection : IEnumerable<HeaderValue>
	{
		readonly List<HeaderValue> headers = new List<HeaderValue>();
 
		public void Add(string header, string value) {
			headers.Add(new HeaderValue(header, value));
		}

		public HeaderValue[] ToArray() { return headers.ToArray(); }

		IEnumerator<HeaderValue> IEnumerable<HeaderValue>.GetEnumerator() { return headers.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return headers.GetEnumerator(); }
	}

    public static class SimpleAspNetHostExtensions 
    {
        static readonly byte[] EmptyBody = new byte[0];

		public static void Start(this SimpleAspNetHost self) { self.Get("/"); }

        public static SimpleAspNetHostResponse Get(this SimpleAspNetHost self, string url, params HeaderValue[] headers) {
            return self.ProcessRequest("GET", url, headers, EmptyBody);
        }

        public static SimpleAspNetHostResponse Get(this SimpleAspNetHost self, string url, HttpHeaderCollection headers) {
            return self.ProcessRequest("GET", url, headers.ToArray(), EmptyBody);
        }

        public static SimpleAspNetHostResponse Put(this SimpleAspNetHost self, string url, Action<IMessageBuilder> createBody) {
            return self.ProcessMessage("PUT", url, createBody);
        }

        public static SimpleAspNetHostResponse Post(this SimpleAspNetHost self, string url, Action<IMessageBuilder> createBody) {
            return self.ProcessMessage("POST", url, createBody);
        }

        static SimpleAspNetHostResponse ProcessMessage(this SimpleAspNetHost self, string method, string url,  Action<IMessageBuilder> createBody) {
            var body = new SimpleMessageBuilder();
            createBody(body);
			var headers = new HttpHeaderCollection { 
				{ HttpHeaders.ContentLength, body.Data.Length.ToString() }
			};
			foreach(var item in body.Headers)
				headers.Add(item.Key, item.Value);
			return self.ProcessRequest(method, url, headers.ToArray(), body.Data);
        }
    }
}
