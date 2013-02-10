using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Applications.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetApplicationQuery : SessionAccessBase, IGetApplicationQuery
    {
        private readonly IAuthorisationManager _authorisationManager;

        public GetApplicationQuery(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public GetApplicationResponse Invoke(GetApplicationRequest request)
        {
            Trace("Starting...");

            string applicationId = Application.GetId(request.Id);
            string organisationId = Organisation.GetId(request.OrganisationId);

            var organisation = MasterLoad<Organisation>(organisationId);

            Session.SetOrganisation(organisation);

            var application = Load<Application>(applicationId);

            if(application != null)
            {
                _authorisationManager.Authorise(application, request.CurrentUser);
            }

            return new GetApplicationResponse
            {
                Application = application,
                Organisation = organisation,
            };
        }
    }

    public interface IGetApplicationQuery : IQuery<GetApplicationRequest, GetApplicationResponse>
    { }

    [ProtoContract]
    public class GetApplicationResponse
    {
        [ProtoMember(1)]
        public Application Application { get; set; }
        [ProtoMember(2)]
        public Organisation Organisation { get; set; }
    }

    public class GetApplicationRequest : CacheableOrganisationRequestBase<GetApplicationResponse>
    {
        public string Id { get; set; }
        public string OrganisationId { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Applications.Key(OrganisationId, Id);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Applications;
        }
    }
}
