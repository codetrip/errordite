using System.Net;

namespace CodeTrip.Core.Web
{
    /// <summary>
    /// Interface which represents a fluent WebRequest
    /// </summary>
    public interface IFluentSynchronousWebRequest
    {
        /// <summary>
        /// Adds a parameter to the request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IFluentSynchronousWebRequest AddParam(string name, string value);

        /// <summary>
        /// Adds a header to the request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IFluentSynchronousWebRequest AddHeader(string name, string value);

        /// <summary>
        /// Adds a header to the request
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        /// <returns></returns>
        IFluentSynchronousWebRequest AddCookie(Cookie cookie);

        /// <summary>
        /// Indicates how long we should wait in milliseconds for the response before timing out, defaults to 10000 milliseconds (10 seconds)
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout millisecondsseconds.</param>
        /// <returns></returns>
        IFluentSynchronousWebRequest TimeoutIn(int timeoutMilliseconds);

        /// <summary>
        /// Set the HttpMethod for the request, defaults to GET
        /// </summary>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <returns></returns>
        IFluentSynchronousWebRequest WithMethod(string httpMethod);

        /// <summary>
        /// Set the HttpMethod for the request, defaults to GET
        /// </summary>
        /// <param name="referer">The referer.</param>
        /// <returns></returns>
        IFluentSynchronousWebRequest FromReferer(string referer);

        /// <summary>
        /// The User-Agent
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        IFluentSynchronousWebRequest WithUserAgent(string userAgent);

        /// <summary>
        /// The User-Agent
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        IFluentSynchronousWebRequest Raw(string content);

        /// <summary>
        /// The User-Agent
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        IFluentSynchronousWebRequest WithContentType(string contentType);

        /// <summary>
        /// Final fluent method, invokes the URI and returns the WebResponse with the body as a string
        /// </summary>
        /// <returns></returns>
        SynchronousWebResponse GetResponse();
        
        /// <summary>
        /// Final fluent method, invokes the URI and returns the WebResponse with the body as a stream
        /// </summary>
        /// <returns></returns>
        SynchronousWebResponse GetResponseStream();
    }
}