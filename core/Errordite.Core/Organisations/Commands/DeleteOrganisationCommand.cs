using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using System.Linq;
using Errordite.Core.Session;
using Raven.Abstractions.Data;
using ProductionProfiler.Core.Extensions;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteOrganisationCommand : SessionAccessBase, IDeleteOrganisationCommand
    {
        private readonly IGetApplicationsQuery _getApplicationsQuery;
        private readonly ErrorditeConfiguration _configuration;
        private readonly IDeleteApplicationCommand _deleteApplicationCommand;

        public DeleteOrganisationCommand(IGetApplicationsQuery getApplicationsQuery, 
            ErrorditeConfiguration configuration, 
            IDeleteApplicationCommand deleteApplicationCommand)
        {
            _getApplicationsQuery = getApplicationsQuery;
            _configuration = configuration;
            _deleteApplicationCommand = deleteApplicationCommand;
        }

        public DeleteOrganisationResponse Invoke(DeleteOrganisationRequest request)
        {
            Session.MasterRavenDatabaseCommands.DeleteByIndex(
                CoreConstants.IndexNames.UserOrganisationMappings, new IndexQuery
                    {
                        Query = "OrganisationId:{0}".FormatWith(Organisation.GetId(request.OrganisationId))
                    }, true);

            Session.MasterRavenDatabaseCommands.DeleteByIndex(
                CoreConstants.IndexNames.Organisations, new IndexQuery
                {
                    Query = "Id:{0}".FormatWith(Organisation.GetId(request.OrganisationId))
                }, true);

            Session.SynchroniseIndexes<UserOrganisationMappings, Organisations_Search>();

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