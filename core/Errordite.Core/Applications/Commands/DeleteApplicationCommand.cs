using System;
using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
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

			DeleteByIndex(CoreConstants.IndexNames.Errors, applicationId);
			DeleteByIndex(CoreConstants.IndexNames.Issues, applicationId);
			DeleteByIndex(CoreConstants.IndexNames.IssueDailyCount, applicationId);
			DeleteByIndex(CoreConstants.IndexNames.OrganisationIssueDailyCount, applicationId);
			DeleteByIndex(CoreConstants.IndexNames.IssueHistory, applicationId);

            if (!request.JustDeleteErrors)
                Delete(application);

            Session.SynchroniseIndexes<Indexing.Errors, Indexing.Issues, Indexing.Applications>();
            Session.AddCommitAction(new FlushApplicationCacheCommitAction(_configuration, request.CurrentUser.ActiveOrganisation, application.FriendlyId));

            return new DeleteApplicationResponse(false, request.ApplicationId, application.OrganisationId)
            {
                Status = DeleteApplicationStatus.Ok
            };
        }

		/// <summary>
		/// aseems that if there are no documents in a index we get an invalid operation exception, swollowing it
		/// </summary>
		/// <param name="indexName"></param>
		/// <param name="applicationId"></param>
		private void DeleteByIndex(string indexName, string applicationId)
		{
			Session.RavenDatabaseCommands.DeleteByIndex(indexName, new IndexQuery
			{
				Query = "ApplicationId:{0}".FormatWith(applicationId)
			}, true);
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
