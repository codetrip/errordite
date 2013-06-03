using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Errordite.Core.Web
{
    /// <summary>
    /// Simple entity which represents the response to a WebRequest
    /// </summary>
    public class SynchronousWebResponse
    {
        private SynchronousWebResponse()
        {}

        /// <summary>
        /// Collection of response headers
        /// </summary>
        public WebHeaderCollection Headers { get; set; }

        /// <summary>
        /// The body of the response
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The body of the response as a stream
        /// </summary>
        public Stream BodyStream { get; set; }

        /// <summary>
        /// The response content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The response content type
        /// </summary>
        public string ContentEncoding { get; set; }

        /// <summary>
        /// The response content type
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// How long did it take to retrieve the response (this does not include parsing the response)
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// The cookies in the response
        /// </summary>
        public CookieCollection Cookies { get; set;}

        /// <summary>
        /// The response content type
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Create the response from the request
        /// </summary>
        /// <returns></returns>
        public static SynchronousWebResponse Create(WebRequest request, bool asStream)
        {
            var webResponse = new SynchronousWebResponse();

            Stopwatch watch = Stopwatch.StartNew();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                watch.Stop();

                webResponse.ElapsedMilliseconds = watch.ElapsedMilliseconds;
                webResponse.ContentEncoding = response.ContentEncoding;
                webResponse.ContentLength = response.ContentLength;
                webResponse.ContentType = response.ContentType;
                webResponse.Headers = response.Headers;
                webResponse.Status = response.StatusCode;
                webResponse.Cookies = response.Cookies;

                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                        return webResponse;

                    if (!asStream)
                    {
                        using (var readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            webResponse.Body = readStream.ReadToEnd();
                        }
                    }
                    else
                    {
                        webResponse.BodyStream = new MemoryStream();
                        responseStream.CopyTo(webResponse.BodyStream);
                        webResponse.BodyStream.Seek(0, SeekOrigin.Begin);
                    }
                }
            }

            return webResponse;
        }
    }
}
