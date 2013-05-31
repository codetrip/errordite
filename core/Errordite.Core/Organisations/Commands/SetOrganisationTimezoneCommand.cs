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
    public class UpdateOrganisationCommand : SessionAccessBase, IUpdateOrganisationCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public UpdateOrganisationCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public UpdateOrganisationResponse Invoke(UpdateOrganisationRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new UpdateOrganisationResponse(request.OrganisationId, null, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            //TODO - admin auth
            _authorisationManager.Authorise(organisation, request.CurrentUser);

            organisation.TimezoneId = request.TimezoneId;
	        organisation.Name = request.Name;
            organisation.PrimaryUserId = User.GetId(request.PrimaryUserId);

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

            return new UpdateOrganisationResponse(organisation.Id, request.CurrentUser.Email);
        }
    }

    public interface IUpdateOrganisationCommand : ICommand<UpdateOrganisationRequest, UpdateOrganisationResponse>
    { }

    public class UpdateOrganisationRequest
    {
        public string OrganisationId { get; set; }
        public User CurrentUser { get; set; }
		public string TimezoneId { get; set; }
        public string Name { get; set; }
        public string PrimaryUserId { get; set; }
    }

    public class UpdateOrganisationResponse : CacheInvalidationResponseBase
    {
		private readonly string _organisationId;
		private readonly string _email;

        public UpdateOrganisationResponse(string organisationId, string email, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
	        _email = email;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
			return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId, _email);
        }
    }
}