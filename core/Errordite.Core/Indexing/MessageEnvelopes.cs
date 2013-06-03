using System.Linq;
using Errordite.Core.Messaging;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class MessageEnvelopes : AbstractIndexCreationTask<MessageEnvelope>
    {
		public MessageEnvelopes()
         {
			 Map = envelopes => from e in envelopes
                                select new
	                               {
		                               e.GeneratedOnUtc,
									   e.OrganisationId,
									   e.Service
	                               };
         }
    }
}