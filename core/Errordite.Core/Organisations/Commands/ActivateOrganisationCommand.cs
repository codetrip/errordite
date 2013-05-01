using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.Paging;
using Errordite.Core.Applications.Queries;
using Errordite.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session.Actions;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

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
            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(request.OrganisationId);

            if (organisation == null)
                return new ActivateOrganisationResponse(request.OrganisationId, true);

            organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);

            if (organisation.Status != OrganisationStatus.Suspended)
            {
                return new ActivateOrganisationResponse(organisation.Id, true)
                {
                    Status = ActivateOrganisationStatus.AccountAlreadyActivate
                };
            }

            organisation.Status = OrganisationStatus.Active;

            using (Session.SwitchOrg(organisation))
            {
                //disable all applications
                var applications = _getApplicationsQuery.Invoke(new GetApplicationsRequest
                {
                    OrganisationId = organisation.Id,
                    Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
                }).Applications;

                foreach (var application in applications.Items)
                {
                    application.IsActive = true;
                    Session.AddCommitAction(new FlushApplicationCacheCommitAction(_configuration, organisation, application.Id));
                }
            }

            Session.AddCommitAction(new FlushOrganisationCacheCommitAction(_configuration, organisation));

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