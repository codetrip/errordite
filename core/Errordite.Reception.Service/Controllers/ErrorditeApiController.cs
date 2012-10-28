using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Reception.Service.Controllers
{
    public abstract class ErrorditeApiController : ApiController
    {
        protected readonly IAppSession Session;
        protected readonly IComponentAuditor Auditor;
        protected readonly IGetOrganisationQuery GetOrganisation;

        protected ErrorditeApiController()
        {
            Auditor = ObjectFactory.GetObject<IComponentAuditor>();
            Session = ObjectFactory.GetObject<IAppSession>();
            GetOrganisation = ObjectFactory.GetObject<IGetOrganisationQuery>();
        }

        //TODO: do this with an action filter maybe
        protected void SetOrganisation(string orgId)
        {
            var organisation = GetOrganisation.Invoke(new GetOrganisationRequest
            {
                OrganisationId = orgId
            }).Organisation;

            Session.SetOrganisation(organisation);
        }
    }
}