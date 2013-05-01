using System;
using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
	public class OrganisationDocument
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string RavenInstanceId { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; }
        public OrganisationStatus OrganisationStatus { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime? CurrentPeriodEndDate { get; set; }
	}

	public class Organisations : AbstractIndexCreationTask<Organisation, OrganisationDocument>
    {
        public Organisations()
        {
            Map = organisations => from o in organisations
                select new
                {
                    o.Id,
                    o.Name,
                    o.RavenInstanceId,
					SubscriptionStatus = o.Subscription.Status,
					CreatedOnDate = o.CreatedOnUtc.Date,
                    OrganisationStatus = o.Status,
                    o.Subscription.CurrentPeriodEndDate
                };
        }
    }
}