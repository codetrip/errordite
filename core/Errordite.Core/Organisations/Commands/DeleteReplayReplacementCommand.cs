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
using System.Linq;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteReplayReplacementCommand : SessionAccessBase, IDeleteReplayReplacementCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public DeleteReplayReplacementCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public DeleteReplayReplacementResponse Invoke(DeleteReplayReplacementRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new DeleteReplayReplacementResponse(request.OrganisationId, null, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            _authorisationManager.Authorise(organisation, request.CurrentUser);

			if (organisation.ReplayReplacements == null)
			{
				return new DeleteReplayReplacementResponse(organisation.Id, request.CurrentUser.Email, true);
			}

	        var replacement = organisation.ReplayReplacements.FirstOrDefault(r => r.Id == request.Id);

			if (replacement == null)
			{
				return new DeleteReplayReplacementResponse(organisation.Id, request.CurrentUser.Email, true);
			}

	        organisation.ReplayReplacements.Remove(replacement);

            return new DeleteReplayReplacementResponse(organisation.Id, request.CurrentUser.Email);
        }
    }

    public interface IDeleteReplayReplacementCommand : ICommand<DeleteReplayReplacementRequest, DeleteReplayReplacementResponse>
    { }

	public class DeleteReplayReplacementRequest : OrganisationRequestBase
    {
        public Guid Id { get; set; }
		public string OrganisationId { get; set; }
    }

    public class DeleteReplayReplacementResponse : CacheInvalidationResponseBase
    {
		private readonly string _organisationId;
		private readonly string _email;

        public DeleteReplayReplacementResponse(string organisationId, string email, bool ignoreCache = false)
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