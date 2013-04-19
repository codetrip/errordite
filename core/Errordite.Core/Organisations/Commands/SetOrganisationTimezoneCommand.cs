using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class SetOrganisationTimezoneCommand : SessionAccessBase, ISetOrganisationTimezoneCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public SetOrganisationTimezoneCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public SetOrganisationTimezoneResponse Invoke(SetOrganisationTimezoneRequest request)
        {
            var organisation = MasterLoad<Organisation>(request.OrganisationId);

            //TODO - admin auth
            _authorisationManager.Authorise(organisation, request.CurrentUser);

            organisation.TimezoneId = request.TimezoneId;

            return new SetOrganisationTimezoneResponse(organisation.Id);
        }
    }

    public interface ISetOrganisationTimezoneCommand : ICommand<SetOrganisationTimezoneRequest, SetOrganisationTimezoneResponse>
    { }

    public class SetOrganisationTimezoneRequest
    {
        public string OrganisationId { get; set; }
        public User CurrentUser { get; set; }
        public string TimezoneId { get; set; }
    }

    public class SetOrganisationTimezoneResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;

        public SetOrganisationTimezoneResponse(string organisationId)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId);
        }
    }

}