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
    public class SetOrganisationCampfireSettingsCommand : SessionAccessBase, ISetOrganisationCampfireSettingsCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public SetOrganisationCampfireSettingsCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public SetOrganisationCampfireSettingsResponse Invoke(SetOrganisationCampfireSettingsRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new SetOrganisationCampfireSettingsResponse(request.OrganisationId, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            _authorisationManager.Authorise(organisation, request.CurrentUser);

	        organisation.CampfireDetails = new CampfireDetails
		    {
			    Company = request.CampfireCompany,
				Token = request.CampfireToken,
		    };

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

            return new SetOrganisationCampfireSettingsResponse(organisation.Id);
        }
    }

    public interface ISetOrganisationCampfireSettingsCommand : ICommand<SetOrganisationCampfireSettingsRequest, SetOrganisationCampfireSettingsResponse>
    { }

    public class SetOrganisationCampfireSettingsRequest
    {
        public string OrganisationId { get; set; }
		public string CampfireToken { get; set; }
		public string CampfireCompany { get; set; }
		public User CurrentUser { get; set; }
    }

    public class SetOrganisationCampfireSettingsResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;

        public SetOrganisationCampfireSettingsResponse(string organisationId, bool ignoreCache = false)
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