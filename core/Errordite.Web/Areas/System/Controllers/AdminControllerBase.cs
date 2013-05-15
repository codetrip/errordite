using System;
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;
using Errordite.Web.Controllers;

namespace Errordite.Web.Areas.System.Controllers
{
    public abstract class AdminControllerBase : ErrorditeController
    {
        public IGetOrganisationQuery GetOrganisationQuery { protected get; set; }
        public IAppSession AppSession { protected get; set; }
        
        protected IDisposable SwitchOrgScope(string organisationId)
        {
            if (AppContext.CurrentUser.Role != UserRole.SuperUser)
            {
                throw new ErrorditeNotSuperUserException();
            }

            var org =
               GetOrganisationQuery.Invoke(new GetOrganisationRequest() { OrganisationId = organisationId })
                                   .Organisation;

            return AppSession.SwitchOrg(org);    
        }
    }

    public class ErrorditeNotSuperUserException : Exception
    {
    }
}