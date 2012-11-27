using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Applications.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class EditApplicationCommand : SessionAccessBase, IEditApplicationCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public EditApplicationCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public EditApplicationResponse Invoke(EditApplicationRequest request)
        {
            Trace("Starting...");

            var applicationId = Application.GetId(request.ApplicationId);

            var existingApplication = Session.Raven.Query<Application, Applications_Search>().Count(o => o.OrganisationId == request.CurrentUser.OrganisationId && o.Name == request.Name && o.Id != applicationId);

            if (existingApplication > 0)
            {
                return new EditApplicationResponse(true)
                {
                    Status = EditApplicationStatus.ApplicationNameExists
                };
            }

            var application = Load<Application>(applicationId);

            if (application == null)
            {
                return new EditApplicationResponse(true)
                {
                    Status = EditApplicationStatus.ApplicationNotFound
                };
            }

            //make sure user is authorised to edit this entity
            _authorisationManager.Authorise(application, request.CurrentUser);

            application.Name = request.Name;
            application.IsActive = request.IsActive;
            application.DefaultUserId = User.GetId(request.UserId);
            application.MatchRuleFactoryId = request.MatchRuleFactoryId;
            application.NotificationGroups = request.NotificationGroups;
            application.HipChatRoomId = request.HipChatRoomId;
            application.HipChatAuthToken = request.HipChatAuthToken;

            Session.SynchroniseIndexes<Applications_Search>();

            return new EditApplicationResponse(false, request.ApplicationId, request.CurrentUser.OrganisationId)
            {
                Status = EditApplicationStatus.Ok
            };
        }
    }

    public interface IEditApplicationCommand : ICommand<EditApplicationRequest, EditApplicationResponse>
    {}

    public class EditApplicationResponse : CacheInvalidationResponseBase
    {
        private readonly string _applicationId;
        private readonly string _organisationId;

        public EditApplicationResponse(bool ignoreCache, string applicationId = "", string organisationId = "")
            : base(ignoreCache)
        {
            _applicationId = applicationId;
            _organisationId = organisationId;
        }

        public EditApplicationStatus Status { get; set; }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetApplicationInvalidationItems(_organisationId, _applicationId);
        }
    }

    public class EditApplicationRequest : OrganisationRequestBase
    {
        public string ApplicationId { get; set; }
        public string MatchRuleFactoryId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public int? HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
        public bool IsActive { get; set; }
        public List<string> NotificationGroups { get; set; }
    }

    public enum EditApplicationStatus
    {
        Ok,
        ApplicationNotFound,
        ApplicationNameExists
    }
}
