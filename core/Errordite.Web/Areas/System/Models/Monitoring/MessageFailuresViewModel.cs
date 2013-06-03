using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Messaging;
using Errordite.Core.Paging;

namespace Errordite.Web.Areas.System.Models.Monitoring
{
	public class MessageFailuresViewModel : MessageFailuresPostModel
	{
		public IEnumerable<MessageEnvelope> Envelopes { get; set; }
		public PagingViewModel Paging { get; set; }
		public IEnumerable<SelectListItem> Services { get; set; }
	}

	public class MessageFailuresPostModel
	{
		public string OrganisationId { get; set; }
		public Service? Service { get; set; }
	}

    public class MessageFailuresActionPostModel
    {
        public List<string> EnvelopeIds { get; set; }
        public string OrgId { get; set; }
        public Service? Svc { get; set; }
    }
}