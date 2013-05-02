using Errordite.Core.Interfaces;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Organisations.Commands
{
    public class GetPlanStatusForOrganisationQuery : SessionAccessBase, IGetPlanStatusForOrganisationQuery
    {
		private readonly IGetOrganisationStatisticsQuery _getOrganisationStatisticsQuery;

	    public GetPlanStatusForOrganisationQuery(IGetOrganisationStatisticsQuery getOrganisationStatisticsQuery)
	    {
		    _getOrganisationStatisticsQuery = getOrganisationStatisticsQuery;
	    }

	    public GetPlanStatusForOrganisationResponse Invoke(GetPlanStatusForOrganisationRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.PaymentPlanId)
                    .Load<Organisation>(request.OrganisationId);

		    if (organisation == null || organisation.Status != OrganisationStatus.Active)
			    return new GetPlanStatusForOrganisationResponse();
					
            organisation.PaymentPlan = MasterLoad<PaymentPlan>(organisation.PaymentPlanId);

            if (organisation.Status != OrganisationStatus.Suspended)
            {
	            return new GetPlanStatusForOrganisationResponse();
            }


            return new GetPlanStatusForOrganisationResponse()
            {
            };
        }
    }

    public interface IGetPlanStatusForOrganisationQuery : ICommand<GetPlanStatusForOrganisationRequest, GetPlanStatusForOrganisationResponse>
    { }

    public class GetPlanStatusForOrganisationRequest
    {
        public string OrganisationId { get; set; }
    }

    public class GetPlanStatusForOrganisationResponse
    {
	   
    }

	public class PlanQuotas
	{
		public int IssuesExceededBy { get; set; }
		public int UsersExceededBy { get; set; }
		public int ApplicationsExceededBy { get; set; }
	}
}