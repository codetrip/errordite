using System;
using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class AddReplayReplacementCommand : SessionAccessBase, IAddReplayReplacementCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public AddReplayReplacementCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public AddReplayReplacementResponse Invoke(AddReplayReplacementRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new AddReplayReplacementResponse(request.OrganisationId, null, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            _authorisationManager.Authorise(organisation, request.CurrentUser);

            if (organisation.ReplayReplacements == null)
	            organisation.ReplayReplacements = new List<ReplayReplacement>();

			organisation.ReplayReplacements.Add(new ReplayReplacement
			{
				Field = request.Field,
				Find = request.Find,
				Replace = request.Replace,
				Id = Guid.NewGuid()
			});

            return new AddReplayReplacementResponse(organisation.Id, request.CurrentUser.Email);
        }
    }

    public interface IAddReplayReplacementCommand : ICommand<AddReplayReplacementRequest, AddReplayReplacementResponse>
    { }

	public class AddReplayReplacementRequest : OrganisationRequestBase
    {
        public string Field { get; set; }
		public string Find { get; set; }
		public string Replace { get; set; }
		public string OrganisationId { get; set; }
    }

    public class AddReplayReplacementResponse : CacheInvalidationResponseBase
    {
		private readonly string _organisationId;
		private readonly string _email;

        public AddReplayReplacementResponse(string organisationId, string email, bool ignoreCache = false)
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