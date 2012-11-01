using System.Linq;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using ProtoBuf;
using CodeTrip.Core.Extensions;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationQuery : SessionAccessBase, IGetOrganisationQuery
    {
        public GetOrganisationResponse Invoke(GetOrganisationRequest request)
        {
            Trace("Starting...");

            var organisationId = Organisation.GetId(request.OrganisationId);
            var organisation = 
                Session.MasterRaven
                    .Include<Organisation>(o => o.PaymentPlanId)
                    .Load<Organisation>(organisationId);

            if(organisation != null)
            {
                organisation.PaymentPlan = CentralLoad<PaymentPlan>(organisation.PaymentPlanId);
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
