using System;
using System.Collections.Generic;
using Amazon.SQS;
using Amazon.SQS.Model;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Raven.Abstractions.Data;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteOrganisationCommand : SessionAccessBase, IDeleteOrganisationCommand
    {
        private readonly ErrorditeConfiguration _configuration;
	    private readonly AmazonSQS _amazonSQS;

        public DeleteOrganisationCommand(ErrorditeConfiguration configuration, AmazonSQS amazonSQS)
        {
	        _configuration = configuration;
	        _amazonSQS = amazonSQS;
        }

	    public DeleteOrganisationResponse Invoke(DeleteOrganisationRequest request)
        {
            var organisation = Session.MasterRaven
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new DeleteOrganisationResponse(request.OrganisationId, true);

            Session.MasterRavenDatabaseCommands.DeleteByIndex(
                CoreConstants.IndexNames.UserOrganisationMappings, new IndexQuery
                    {
                        Query = "Organisations:{0}".FormatWith(Organisation.GetId(request.OrganisationId))
                    }, true);

            Session.MasterRavenDatabaseCommands.DeleteByIndex(
                CoreConstants.IndexNames.Organisations, new IndexQuery
                {
                    Query = "Id:{0}".FormatWith(Organisation.GetId(request.OrganisationId))
                }, true);

		    bool queueDeleted = true;
		    try
		    {
				_amazonSQS.DeleteQueue(new DeleteQueueRequest
				{
					QueueUrl = _configuration.GetReceiveQueueAddress(organisation.FriendlyId)
				});
		    }
		    catch (Exception)
		    {
			    queueDeleted = false;
		    }

			var emailInfo = new NonTemplatedEmailInfo
			{
				To = _configuration.AdministratorsEmail,
				Subject = "Errordite: Organisation Deleted",
				Body = "OrganisationId:{0}, QueueDeleted:={1}".FormatWith(organisation.Id, queueDeleted)
			};

			Session.AddCommitAction(new SendMessageCommitAction(emailInfo, _configuration.GetNotificationsQueueAddress()));
			Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));
            Session.SynchroniseIndexes<UserOrganisationMappings, Indexing.Organisations>();

            return new DeleteOrganisationResponse(request.OrganisationId);
        }
    }

    public interface IDeleteOrganisationCommand : ICommand<DeleteOrganisationRequest, DeleteOrganisationResponse>
    { }

    public class DeleteOrganisationRequest
    {
        public string OrganisationId { get; set; }
    }

    public class DeleteOrganisationResponse : CacheInvalidationResponseBase
    {
        private readonly string _organisationId;

        public DeleteOrganisationResponse(string organisationId, bool ignoreCache = false)
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