using System;
using System.Net.Http;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Web;

namespace Errordite.Core.Session.Actions
{
    public class FlushOrganisationCacheCommitAction : SessionCommitAction
    {
        private readonly string _organisationId;
        private readonly ErrorditeConfiguration _configration;

        public FlushOrganisationCacheCommitAction(ErrorditeConfiguration configration, Organisation organisation)
        {
            _configration = configration;
            _organisationId = organisationId;
        }

        public override void Execute(IAppSession session)
        {
            session.ReceiveServiceHttpClient.DeleteAsync("cache");

            foreach (var endpoint in _configration.ReceiveWebEndpoints.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(endpoint)
                };

                client.DeleteAsync("cache/flush?organisationId={0}".FormatWith(_organisationId));
            }

            var uriBuilder = new UriBuilder(organisation.RavenInstance.ReceiveServiceEndpoint);

            if (!uriBuilder.Path.EndsWith("/"))
                uriBuilder.Path += "/";

            uriBuilder.Path += "{0}/".FormatWith(organisation.FriendlyId);

            _receiveServiceHttpClient = new HttpClient(new LoggingHttpMessageHandler(_auditor)) { BaseAddress = uriBuilder.Uri };
        }
    }
}
