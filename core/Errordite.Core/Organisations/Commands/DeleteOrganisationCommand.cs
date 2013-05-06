using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Master;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Raven.Abstractions.Data;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class DeleteOrganisationCommand : SessionAccessBase, IDeleteOrganisationCommand
    {
        private readonly ErrorditeConfiguration _configuration;

        public DeleteOrganisationCommand(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DeleteOrganisationResponse Invoke(DeleteOrganisationRequest request)
        {
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new DeleteOrganisationResponse(request.OrganisationId, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

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