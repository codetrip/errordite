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
	public class NotifyOrganisationsWithExceededQuotasTask : ComponentBase, ITask
	{
        private readonly IAppSession _session;
        private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;
        private readonly IGetAvailablePaymentPlansQuery _getAvailablePaymentPlansQuery;

		public NotifyOrganisationsWithExceededQuotasTask(IAppSession session, 
            IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery, 
            IGetAvailablePaymentPlansQuery getAvailablePaymentPlansQuery)
	    {
	        _session = session;
	        _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
	        _getAvailablePaymentPlansQuery = getAvailablePaymentPlansQuery;
	    }

	    public void Execute(string ravenInstanceId)
		{
			//var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
			//							.Where(o => o.OrganisationStatus == OrganisationStatus.PlanQuotaExceeded && o.QuotasExceededReminders < 3)
			//							.As<Organisation>()
			//							.ToList();

			//if (organisations.Any())
			//{
			//	Trace("Found {0} active organisations", organisations.Count);

			//	var plans = _getAvailablePaymentPlansQuery.Invoke(new GetAvailablePaymentPlansRequest()).Plans;

			//	foreach (var organisation in organisations)
			//	{
			//		using (_session.SwitchOrg(organisation))
			//		{
                        
			//		}
			//	}
			//}
		}
	}
}
