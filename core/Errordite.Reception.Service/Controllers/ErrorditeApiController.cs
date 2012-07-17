using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Core.Session;

namespace Errordite.Reception.Service.Controllers
{
    public abstract class ErrorditeApiController : ApiController
    {
        protected readonly IAppSession _session;
        protected readonly IComponentAuditor _auditor;

        protected ErrorditeApiController()
        {
            _auditor = ObjectFactory.GetObject<IComponentAuditor>();
            _session = ObjectFactory.GetObject<IAppSession>();
        }
    }
}