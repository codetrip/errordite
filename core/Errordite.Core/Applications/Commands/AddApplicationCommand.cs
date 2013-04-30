using System.Collections.Generic;
using System.Web.Security;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Extensions;
using Raven.Client;

namespace Errordite.Core.Applications.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class AddApplicationCommand : SessionAccessBase, IAddApplicationCommand
    {
        private readonly IEncryptor _encryptor;

        public AddApplicationCommand(IEncryptor encryptor)
        {
            _encryptor = encryptor;
        }

        public AddApplicationResponse Invoke(AddApplicationRequest request)
        {
            Trace("Starting...");

            var existingApplication = Session.Raven.Query<Application, Indexing.Applications>().FirstOrDefault(a => a.Name == request.Name);

            if (existingApplication != null)
            {
                return new AddApplicationResponse(true)
                {
                    Status = AddApplicationStatus.ApplicationExists
                };
            }

            RavenQueryStatistics stats;
            var applications = Session.Raven.Query<Application, Indexing.Applications>()
                .Statistics(out stats)
                .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                .Take(0);

            if (stats.TotalResults >= request.CurrentUser.Organisation.PaymentPlan.MaximumApplications)
            {
                return new AddApplicationResponse(true)
                {
                    Status = AddApplicationStatus.PlanThresholdReached
                };
            }

            var application = new Application
            {
                Name = request.Name,
                OrganisationId = request.CurrentUser.OrganisationId,
                IsActive = request.IsActive,
                MatchRuleFactoryId = request.MatchRuleFactoryId,
                DefaultUserId = User.GetId(request.UserId),
                NotificationGroups = request.NotificationGroups,
                HipChatRoomId = request.HipChatRoomId,
                HipChatAuthToken = request.HipChatAuthToken,
                TokenSalt = Membership.GeneratePassword(4, 0).Replace("|", "^"),
                Version = request.Version,
                TimezoneId = request.CurrentUser.Organisation.TimezoneId
            };

            Store(application);

            application.Token = _encryptor.Encrypt("{0}|{1}|{2}".FormatWith(application.FriendlyId, application.OrganisationId.GetFriendlyId(), application.TokenSalt));

            Session.SynchroniseIndexes<Indexing.Applications>();
            
            return new AddApplicationResponse(false, request.CurrentUser.OrganisationId)
            {
                AuthenticationToken = application.Token,
                Status = AddApplicationStatus.Ok,
                ApplicationId = application.Id,
            };
        }
    }

    public interface IAddApplicationCommand : ICommand<AddApplicationRequest, AddApplicationResponse>
    {}

    public class AddApplicationResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;
        public string AuthenticationToken { get; set; }
        public AddApplicationStatus Status { get; set; }

        public string ApplicationId { get; set; }

        public AddApplicationResponse(bool ignoreCache, string organisationId = "")
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return Caching.CacheInvalidation.GetApplicationInvalidationItems(_organisationId);
        }
    }

    public class AddApplicationRequest : OrganisationRequestBase
    {
        public string MatchRuleFactoryId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public int? HipChatRoomId { get; set; }
        public string HipChatAuthToken { get; set; }
        public string WebHookUri { get; set; }
        public bool IsActive { get; set; }
        public string Version { get; set; }
        public List<string> NotificationGroups { get; set; }
    }

    public enum AddApplicationStatus
    {
        Ok,
        ApplicationExists,
        PlanThresholdReached
    }
}
