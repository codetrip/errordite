using Errordite.Core;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;

namespace Errordite.Tasks.Tasks
{
	public class IdentifyOrganisationsWithExceededQuotasTask : ComponentBase, ITask
	{
        private readonly IAppSession _session;
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;

	    public IdentifyOrganisationsWithExceededQuotasTask(IAppSession session, 
            IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery)
	    {
	        _session = session;
	        _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
	        _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
	    }

	    public void Execute(string ravenInstanceId)
		{
			var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
			                            .Where(o => o.OrganisationStatus == OrganisationStatus.Active && o.SubscriptionStatus == SubscriptionStatus.Active)
			                            .As<Organisation>()
			                            .ToList();

			if (organisations.Any())
			{
				Trace("Found {0} active organisations", organisations.Count);

			    var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

				foreach (var organisation in organisations)
				{
                    using (_session.SwitchOrg(organisation))
                    {
                        var stats = _getOrganisationStatisticsQuery.Invoke(new GetOrganisationStatisticsRequest()).Statistics;
                        var plan = plans.FirstOrDefault(p => p.Id == organisation.PaymentPlanId && !p.IsFreeTier);

                        if (plan != null)
                        {
                            var quotas = PlanQuotas.FromStats(stats, plan);

                            if (quotas.IssuesExceededBy > 0)
                            {
                                organisation.Status = OrganisationStatus.PlanQuotaExceeded;
                            }
                        }
                    }
				}
			}
		}
	}
}
