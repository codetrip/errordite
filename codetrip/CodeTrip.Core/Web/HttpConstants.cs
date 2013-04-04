
namespace CodeTrip.Core.Web
{
    public class HttpConstants
    {
        public static class HttpMethods
        {
            public const string Get = "get";
            public const string Post = "post";
            public const string Head = "head";
        }

        public static class HttpHeaders
        {
            public const string AcceptRanges = "Accept-Ranges";
            public const string AcceptRangesBytes = "bytes";

            public const string ContentType = "Content-Type";
            public const string ContentRange = "Content-Range";
            public const string ContentLength = "Content-Length";
            public const string ContentDisposition = "Content-Disposition";
            public const string EntityTag = "ETag";

            public const string LastModified = "Last-Modified";

            public const string Range = "Range";

            public const string IfRange = "If-Range";
            public const string IfMatch = "If-Match";
            public const string IfNoneMatch = "If-None-Match";
            public const string IfModifiedSince = "If-Modified-Since";
            public const string IfUnmodifiedSince = "If-Unmodified-Since";
            public const string UnlessModifiedSince = "Unless-Modified-Since";

            public static string ContentDispositionInline(string filename)
            {
                return string.Format("inline; filename={0}", filename);
            }
            public static string ContentDispositionAttachment(string filename)
            {
                return string.Format("attachment; filename={0}", filename);
            }
        }

        public static class ContentTypes
        {
            public const string MultipartBoundary = "<q1w2e3r4t5y6u7i8o9p0>";
            public const string Multipart = "multipart/byteranges; boundary=" + MultipartBoundary;
            public const string FormUrlEncoded = "application/x-www-form-urlencoded";
            public const string Plain = "text/plain";
            public const string Xml = "text/xml";
            public const string Html = "text/html";
            public const string Asx = "video/x-ms-asf";
            public const string Jpeg = "image/jpeg";
            public const string Gif = "image/gif";
            public const string Png = "image/png";
            public const string Bmp = "image/bmp";
            public const string Flv = "video/x-flv";
            public const string Json = "application/json";
            public const string Swf = "application/x-shockwave-flash";
        }
    }
}
