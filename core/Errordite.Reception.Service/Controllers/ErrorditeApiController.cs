using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Reception.Service.Controllers
{
    public abstract class ErrorditeApiController : ApiController
    {
        public IAppSession Session { protected get; set; }
        public IComponentAuditor Auditor { protected get; set; }
        public IGetOrganisationQuery GetOrganisation { protected get; set; }

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