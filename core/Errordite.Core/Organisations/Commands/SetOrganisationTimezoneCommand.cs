using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class SetOrganisationTimezoneCommand : SessionAccessBase, ISetOrganisationTimezoneCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public SetOrganisationTimezoneCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public SetOrganisationTimezoneResponse Invoke(SetOrganisationTimezoneRequest request)
        {
            var organisation = MasterLoad<Organisation>(request.OrganisationId);

            //TODO - admin auth
            _authorisationManager.Authorise(organisation, request.CurrentUser);

            organisation.TimezoneId = request.TimezoneId;

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

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