using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Applications.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class EditApplicationCommand : SessionAccessBase, IEditApplicationCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public EditApplicationCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public EditApplicationResponse Invoke(EditApplicationRequest request)
        {
            Trace("Starting...");

            var applicationId = Application.GetId(request.ApplicationId);

            var existingApplication = Session.Raven.Query<Application, Applications_Search>().Count(o => o.Name == request.Name && o.Id != applicationId);

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
            application.Version = request.Version;

            Session.SynchroniseIndexes<Applications_Search>();
            Session.AddCommitAction(new FlushApplicationCacheCommitAction(_configuration, application.OrganisationId.GetFriendlyId(), application.FriendlyId));

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
        public string Version { get; set; }
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
