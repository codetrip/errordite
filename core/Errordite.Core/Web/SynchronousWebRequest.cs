using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;
using Errordite.Core.Extensions;

namespace Errordite.Core.Web
{
    /// <summary>
    /// Simple Fluent wrapper over the System.Web.WebRequest object
    /// </summary>
    public class SynchronousWebRequest : IFluentSynchronousWebRequest
    {
        private int _timeout = 10000;
        private readonly string _requestUri;
        private string _userAgent;
        private string _rawContent;
        private string _contentType;
        private string _httpMethod = HttpConstants.HttpMethods.Get;
        private readonly CookieCollection _requestCookies = new CookieCollection();
        private readonly NameValueCollection _requestParams = new NameValueCollection();
        private readonly WebHeaderCollection _requestHeaders = new WebHeaderCollection();
		private string _referer = string.Empty;
		private string _accept = string.Empty;
		private string _host = string.Empty;
		private string _connection = string.Empty;

        private SynchronousWebRequest(string requestUri)
        {
            _requestUri = requestUri;
        }

        /// <summary>
        /// Initiate the fluent interface of the WebRequest object by specifying the URI you want to invoke
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IFluentSynchronousWebRequest To(string uri)
        {
            return new SynchronousWebRequest(uri);
        }

	    public IFluentSynchronousWebRequest Accept(string accept)
	    {
			_accept = accept;
			return this;
	    }

		public IFluentSynchronousWebRequest Connection(string connection)
		{
			_connection = connection;
			return this;
		}

		public IFluentSynchronousWebRequest Host(string host)
		{
			_host = host;
			return this;
		}

	    public IFluentSynchronousWebRequest FromReferer(string referer)
        {
            _referer = referer;
            return this;
        }

        public IFluentSynchronousWebRequest AddParam(string name, string value)
        {
            _requestParams.Add(name, value);
            return this;
        }

        public IFluentSynchronousWebRequest AddHeader(string name, string value)
        {
			if (value.IsNotNullOrEmpty())
				_requestHeaders.Add(name, value);
            return this;
        }

        public IFluentSynchronousWebRequest AddCookie(Cookie cookie)
        {
            if (cookie != null)
                _requestCookies.Add(cookie);

            return this;
        }

        public IFluentSynchronousWebRequest TimeoutIn(int timeoutMilliseconds)
        {
            _timeout = timeoutMilliseconds;
            return this;
        }

        public IFluentSynchronousWebRequest WithMethod(string httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public IFluentSynchronousWebRequest WithUserAgent(string userAgent)
        {
            _userAgent = userAgent;
            return this;
        }

        public IFluentSynchronousWebRequest WithContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public IFluentSynchronousWebRequest Raw(string content)
        {
            _rawContent = content;
            return this;
        }

        public SynchronousWebResponse GetResponseStream()
        {
            return DoGetResponse(true);
        }

        public SynchronousWebResponse GetResponse()
        {
            return DoGetResponse(false);
        }

        public SynchronousWebResponse DoGetResponse(bool asStream)
        {
            string requestData = EncodeRequestParams();

            HttpWebRequest request;
            if (_httpMethod == HttpConstants.HttpMethods.Post)
            {
                var uri = new Uri(_requestUri);
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Referer = _referer;
                request.Method = HttpConstants.HttpMethods.Post;
                request.ContentType = _contentType.IsNullOrEmpty() ? HttpConstants.ContentTypes.FormUrlEncoded : _contentType;
                request.Timeout = _timeout;
                request.UserAgent = _userAgent;
	            request.Accept = _accept;
	            request.Host = _host;
	            request.Connection = _connection;

                //get the request data byte array
                byte[] bytes = Encoding.UTF8.GetBytes(requestData);

                //set the content length
                request.ContentLength = bytes.Length;

                //write to the request stream
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
            }
            else
            {
                var uri = new Uri(string.Format("{0}{1}{2}",
                    _requestUri,
                    _requestUri.Contains("?") ? "&" : "?",
                    requestData));

                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = _httpMethod;
                request.Referer = _referer;
                request.Timeout = _timeout;
                request.UserAgent = _userAgent;
            }

            //set the cookies to the request
            if (_requestCookies.Count > 0)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(_requestCookies);
            }

            //assign any headers to the request
            if (_requestHeaders.Count > 0)
                request.Headers.Add(_requestHeaders);

            return SynchronousWebResponse.Create(request, asStream);
        }

        #region Helpers

        private string EncodeRequestParams()
        {
            //if this is a raw request, dont encode request data as params, just return the raw content
            if (_rawContent.IsNotNullOrEmpty())
            {
                return _rawContent;
            }

            var requestParams = new StringBuilder();

            for (int i = 0; i < _requestParams.Count; i++)
            {
                if (i > 0)
                    requestParams.Append("&");

                requestParams.Append(_requestParams.GetKey(i));
                requestParams.Append("=");
                requestParams.Append(HttpUtility.UrlEncode(_requestParams.Get(i)));
            }

            return requestParams.ToString();
        }

        #endregion
    }
}
