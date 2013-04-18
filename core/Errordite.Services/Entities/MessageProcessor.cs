using System.Collections.Generic;
using CodeTrip.Core.Queueing;
using Errordite.Core.Messages;
using System.Linq;
using Errordite.Services.Configuration;

namespace Errordite.Services.Entities
{
    public class MessageProcessor
    {
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly List<string> _organisations = new List<string>();
        private OwnThreadQueueHelper<MessageBase> _queueHelper;

        public MessageProcessor(ServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
        }

        public bool ContainsOrganisation(string organisationId)
        {
            return _organisations.Any(o => o == organisationId);
        }

        public bool CanAddOrganisation()
        {
            return _organisations.Count < _serviceConfiguration.MaxOrganisationsPerMessageprocesor;
        }

        public void AddOrganisation(string organisationId)
        {
            _organisations.Add(organisationId);
        }
    }
}
