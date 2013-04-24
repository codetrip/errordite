using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Raven.Abstractions.Data;
using Errordite.Core.Extensions;

namespace Errordite.Core.Applications.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteApplicationCommand : SessionAccessBase, IDeleteApplicationCommand
    {
        private readonly IAuthorisationManager _authorisationManager;
        private readonly ErrorditeConfiguration _configuration;

        public DeleteApplicationCommand(IAuthorisationManager authorisationManager, ErrorditeConfiguration configuration)
        {
            _authorisationManager = authorisationManager;
            _configuration = configuration;
        }

        public DeleteApplicationResponse Invoke(DeleteApplicationRequest request)
        {
            Trace("Starting...");

            var applicationId = Application.GetId(request.ApplicationId);
            var application = Session.Raven.Load<Application>(applicationId);

            if (application == null)
            {
                return new DeleteApplicationResponse(true)
                {
                    Status = DeleteApplicationStatus.ApplicationNotFound
                };
            }

            _authorisationManager.Authorise(application, request.CurrentUser);

			Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.Errors, new IndexQuery
            {
                Query = "ApplicationId:{0}".FormatWith(applicationId)
            }, true);

			Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.Issues, new IndexQuery
            {
                Query = "ApplicationId:{0}".FormatWith(applicationId)
            }, true);

            Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.IssueDailyCount, new IndexQuery
            {
                Query = "ApplicationId:{0}".FormatWith(applicationId)
            }, true);

            Session.RavenDatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.OrganisationIssueDailyCount, new IndexQuery
            {
                Query = "ApplicationId:{0}".FormatWith(applicationId)
            }, true);

            if (!request.JustDeleteErrors)
                Delete(application);

            Session.SynchroniseIndexes<Errors_Search, Issues_Search, Applications_Search>();
            Session.AddCommitAction(new FlushApplicationCacheCommitAction(_configuration, application.OrganisationId.GetFriendlyId(), application.FriendlyId));

            return new DeleteApplicationResponse(false, request.ApplicationId, application.OrganisationId)
            {
                Status = DeleteApplicationStatus.Ok
            };
        }
    }

    public interface IDeleteApplicationCommand : ICommand<DeleteApplicationRequest, DeleteApplicationResponse>
    {}

    public class DeleteApplicationResponse : CacheInvalidationResponseBase
    {
        private readonly string _applicationId;
        private readonly string _organisationId;
        public DeleteApplicationStatus Status { get; set; }

        public DeleteApplicationResponse(bool ignoreCache, string applicationId = "", string organisationId = "")
            : base(ignoreCache)
        {
            _applicationId = applicationId;
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetApplicationInvalidationItems(_organisationId, _applicationId);
        }
    }

    public class DeleteApplicationRequest : OrganisationRequestBase
    {
        public string ApplicationId { get; set; }
        public bool JustDeleteErrors { get; set; }
    }

    public enum DeleteApplicationStatus
    {
        Ok,
        ApplicationNotFound
    }
}
