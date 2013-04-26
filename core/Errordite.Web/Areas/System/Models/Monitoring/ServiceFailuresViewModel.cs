using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Messaging;
using Errordite.Core.Paging;

namespace Errordite.Web.Areas.System.Models.Monitoring
{
	public class ServiceFailuresViewModel : ServiceFailuresPostModel
	{
		public IEnumerable<MessageEnvelope> Envelopes { get; set; }
		public PagingViewModel Paging { get; set; }
		public IEnumerable<SelectListItem> Services { get; set; }
	}

	public class ServiceFailuresPostModel
	{
		public string OrganisationId { get; set; }
		public Service? Service { get; set; }
	}
}