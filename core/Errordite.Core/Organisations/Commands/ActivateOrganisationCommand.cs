using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Paging;
using CodeTrip.Core.Session;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Organisations.Commands
{
    [Interceptor(CacheInvalidationInterceptor.IoCName)]
    public class ActivateOrganisationCommand : SessionAccessBase, IActivateOrganisationCommand
    {
        private readonly IGetApplicationsQuery _getApplicationsQuery;
        private readonly ErrorditeConfiguration _configuration;

        public ActivateOrganisationCommand(IGetApplicationsQuery getApplicationsQuery, ErrorditeConfiguration configuration)
        {
            _getApplicationsQuery = getApplicationsQuery;
            _configuration = configuration;
        }

        public ActivateOrganisationResponse Invoke(ActivateOrganisationRequest request)
        {
            var organisation = Load<Organisation>(request.OrganisationId);

            if (organisation.Status == OrganisationStatus.Active)
            {
                return new ActivateOrganisationResponse(organisation.Id, true)
                {
                    Status = ActivateOrganisationStatus.AccountAlreadyActivate
                };
            }

            organisation.Status = OrganisationStatus.Active;

            //disable all applications
            var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
            {
                OrganisationId = organisation.Id,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
            }).Applications;

            foreach(var application in applications.Items)
            {
                application.IsActive = true;
            }

            return new ActivateOrganisationResponse(organisation.Id)
            {
                Status = ActivateOrganisationStatus.Ok
            };
        }
    }

    public interface IActivateOrganisationCommand : ICommand<ActivateOrganisationRequest, ActivateOrganisationResponse>
    { }

    public class ActivateOrganisationRequest
    {
        public string OrganisationId { get; set; }
    }

    public class ActivateOrganisationResponse : CacheInvalidationResponseBase
    {
        public ActivateOrganisationStatus Status { get; set; }
        private readonly string _organisationId;

        public ActivateOrganisationResponse(string organisationId, bool ignoreCache = false)
            : base(ignoreCache)
        {
            _organisationId = organisationId;
        }

        protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
        {
            return CacheInvalidation.GetOrganisationInvalidationItems(_organisationId);
        }
    }

    public enum ActivateOrganisationStatus
    {
        Ok,
        AccountAlreadyActivate
    }
}