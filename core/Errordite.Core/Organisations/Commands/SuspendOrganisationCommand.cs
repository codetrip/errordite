using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class SuspendOrganisationCommand : SessionAccessBase, ISuspendOrganisationCommand
    {
        private readonly IGetApplicationsQuery _getApplicationsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public SuspendOrganisationCommand(IGetApplicationsQuery getApplicationsQuery, ErrorditeConfiguration configuration)
        {
            _getApplicationsQuery = getApplicationsQuery;
            _configuration = configuration;
        }

        public SuspendOrganisationResponse Invoke(SuspendOrganisationRequest request)
        {
            var organisation = Load<Organisation>(request.OrganisationId);

            if (organisation.Status == OrganisationStatus.Suspended)
            {
                return new SuspendOrganisationResponse(organisation.Id, true)
                {
                    Status = SuspendOrganisationStatus.AccountAlreadySuspended
                };
            }

            organisation.Status = OrganisationStatus.Suspended;
            organisation.SuspendedReason = request.Reason;
            organisation.SuspendedMessage = request.Message;

            //disable all applications
            var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
            {
                OrganisationId = organisation.Id,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
            }).Applications;

            foreach(var application in applications.Items)
            {
                application.IsActive = false;
            }

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

            return new SuspendOrganisationResponse(organisation.Id)
            {
                Status = SuspendOrganisationStatus.Ok
            };
        }
    }

    public interface ISuspendOrganisationCommand : ICommand<SuspendOrganisationRequest, SuspendOrganisationResponse>
    { }

    public class SuspendOrganisationRequest
    {
        public string OrganisationId { get; set; }
        public SuspendedReason Reason { get; set; }
        public string Message { get; set; }
    }

    public class SuspendOrganisationResponse : CacheInvalidationResponseBase
    {
        public SuspendOrganisationStatus Status { get; set; }
        private readonly string _organisationId;

        public SuspendOrganisationResponse(string organisationId, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId);
        }
    }

    public enum SuspendOrganisationStatus
    {
        Ok,
        AccountAlreadySuspended
    }
}