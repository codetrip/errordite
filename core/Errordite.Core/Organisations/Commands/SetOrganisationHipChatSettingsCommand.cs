using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class SetOrganisationHipChatSettingsCommand : SessionAccessBase, ISetOrganisationHipChatSettingsCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public SetOrganisationHipChatSettingsCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public SetOrganisationHipChatSettingsResponse Invoke(SetOrganisationHipChatSettingsRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new SetOrganisationHipChatSettingsResponse(request.OrganisationId, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            _authorisationManager.Authorise(organisation, request.CurrentUser);

	        organisation.HipChatAuthToken = request.HipChatAuthToken;

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

            return new SetOrganisationHipChatSettingsResponse(organisation.Id);
        }
    }

    public interface ISetOrganisationHipChatSettingsCommand : ICommand<SetOrganisationHipChatSettingsRequest, SetOrganisationHipChatSettingsResponse>
    { }

    public class SetOrganisationHipChatSettingsRequest
    {
        public string OrganisationId { get; set; }
		public string HipChatAuthToken { get; set; }
		public User CurrentUser { get; set; }
    }

    public class SetOrganisationHipChatSettingsResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;

        public SetOrganisationHipChatSettingsResponse(string organisationId, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId);
        }
    }

}