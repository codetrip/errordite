
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Models.Errors
{
    public class ExceptionViewModel
    {
        public ExceptionInfo Info { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public bool InnerException { get; set; }

        public ExceptionViewModel(ExceptionInfo info, string url, string userAgent, bool innerException = false)
        {
            Info = info;
            Url = url;
            UserAgent = userAgent;
            InnerException = innerException;
        }
    }
}