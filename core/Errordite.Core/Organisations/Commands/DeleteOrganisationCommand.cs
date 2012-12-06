using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using System.Linq;
using Raven.Abstractions.Data;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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
            var organisation = Load<Organisation>(request.OrganisationId);

            if (organisation == null)
            {
                return new DeleteOrganisationResponse(string.Empty, true);
            }

            var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
            {
                OrganisationId = organisation.Id,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize),
            }).Applications;

            foreach(var application in applications.Items)
            {
                _deleteApplicationCommand.Invoke(new DeleteApplicationRequest
                {
                    ApplicationId = application.Id,
                    CurrentUser = User.System(),
                    JustDeleteErrors = false
                });
            }

            var users = Session.Raven.Query<User, Users_Search>().Where(u => u.OrganisationId == organisation.Id).ToList();

            foreach(var user in users)
            {
                Delete(user);

                //Session.Raven.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex(CoreConstants.IndexNames.UserAlerts, new IndexQuery
                //{
                //    Query = "UserId:{0}".FormatWith(user.Id)
                //}, true);
            }

            var groups = Session.Raven.Query<Group, Groups_Search>().Where(u => u.OrganisationId == organisation.Id).ToList();

            foreach(var group in groups)
            {
                Delete(group);
            }

            Delete(organisation);

            return new DeleteOrganisationResponse(organisation.Id);
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