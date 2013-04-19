using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationQuery : SessionAccessBase, IGetOrganisationQuery
    {
        public GetOrganisationResponse Invoke(GetOrganisationRequest request)
        {
            Trace("Starting...");

            var organisationId = Organisation.GetId(request.OrganisationId);

            var organisation = Session.MasterRaven
                    .Include<Organisation>(o => o.PaymentPlanId)
                    .Include<Organisation>(o => o.RavenInstanceId)
                    .Load<Organisation>(organisationId);

            if (organisation != null)
            {
                organisation.PaymentPlan = MasterLoad<PaymentPlan>(organisation.PaymentPlanId);
                organisation.RavenInstance = MasterLoad<RavenInstance>(organisation.RavenInstanceId);
            }

            return new GetOrganisationResponse
            {
                Organisation = organisation
            };
        }
    }

    public interface IGetOrganisationQuery : IQuery<GetOrganisationRequest, GetOrganisationResponse>
    { }

    [ProtoContract]
    public class GetOrganisationResponse
    {
        [ProtoMember(1)]
        public Organisation Organisation { get; set; }
    }

    public class GetOrganisationRequest : CacheableRequestBase<GetOrganisationResponse>
    {
        public string OrganisationId { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Organisations.Key(OrganisationId);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Organisations;
        }
    }
}
