using System.Net.Http;
using Errordite.Core.Auditing.Entities;

namespace Errordite.Core.Web
{
    public class LoggingHttpMessageHandler : DelegatingHandler
    {
        private readonly IComponentAuditor _auditor;

        public LoggingHttpMessageHandler(IComponentAuditor auditor)
        {
            _auditor = auditor;
            base.InnerHandler = new HttpClientHandler();
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            _auditor.Trace(GetType(), "{0} {1}", request.Method, request.RequestUri.ToString());
            return base.SendAsync(request, cancellationToken);
        }
    }
}