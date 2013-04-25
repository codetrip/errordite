using System;
using System.Net.Http;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session.Actions
{
    public class FlushOrganisationCacheCommitAction : SessionCommitAction
    {
        private readonly Organisation _organisation;
        private readonly ErrorditeConfiguration _configration;

        public FlushOrganisationCacheCommitAction(ErrorditeConfiguration configration, Organisation organisation)
        {
            _configration = configration;
            _organisation = organisation;
        }

        public override void Execute(IAppSession session)
        {
            session.ReceiveHttpClient.DeleteAsync("cache");

            foreach (var endpoint in _configration.ReceiveWebEndpoints.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                using(var client = new HttpClient { BaseAddress = new Uri(endpoint) })
	            {
					var t = client.DeleteAsync("cache/flush?organisationId={0}".FormatWith(_organisation.FriendlyId));
                    t.Wait(5000);
	            }
            }

            using(var eventsClient = new HttpClient
                {
                    BaseAddress = new Uri("{0}:802/api/{1}/".FormatWith(_organisation.RavenInstance.ServicesBaseUrl, _organisation.FriendlyId))
                })
            {
                var task = eventsClient.DeleteAsync("cache");
                task.Wait(5000);
            };
        }
    }
}
