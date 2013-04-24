
using System;
using System.Net.Http;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session.Actions
{
    public class FlushApplicationCacheCommitAction : SessionCommitAction
    {
        private readonly string _applicationId;
        private readonly string _organisationId;
        private readonly ErrorditeConfiguration _configration;

        public FlushApplicationCacheCommitAction(ErrorditeConfiguration configration, string organisationId, string applicationId)
        {
            _applicationId = applicationId;
            _configration = configration;
            _organisationId = organisationId;
        }

        public override void Execute(IAppSession session)
        {
            session.ReceiveServiceHttpClient.DeleteAsync("cache?applicationId={0}".FormatWith(_applicationId));

            foreach (var endpoint in _configration.ReceiveWebEndpoints.Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(endpoint)
                };

                client.DeleteAsync("cache/flush?organisationId={0}&applicationId={1}".FormatWith(_organisationId, _applicationId));
            }
        }
    }
}
