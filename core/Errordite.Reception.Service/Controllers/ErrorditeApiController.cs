using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Reception.Service.Controllers
{
    public abstract class ErrorditeApiController : ApiController
    {
        protected readonly IAppSession _session;
        protected readonly IComponentAuditor _auditor;
        protected readonly IGetOrganisationQuery _getOrganisationQuery;

        protected ErrorditeApiController()
        {
            _auditor = ObjectFactory.GetObject<IComponentAuditor>();
            _session = ObjectFactory.GetObject<IAppSession>();
            _getOrganisationQuery = ObjectFactory.GetObject<IGetOrganisationQuery>();
        }

        //TODO: do this with an action filter maybe
        protected void SetOrg(string orgId)
        {
            var org = _getOrganisationQuery.Invoke(new GetOrganisationRequest() {OrganisationId = orgId}).Organisation;
            _session.SetOrg(org);
        }
    }
}