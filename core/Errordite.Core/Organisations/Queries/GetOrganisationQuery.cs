using System.Linq;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using ProtoBuf;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationQuery : SessionAccessBase, IGetOrganisationQuery
    {
        private readonly IGetPaymentPlansQuery _getPaymentPlansQuery;

        public GetOrganisationQuery(IGetPaymentPlansQuery getPaymentPlansQuery)
        {
            _getPaymentPlansQuery = getPaymentPlansQuery;
        }

        public GetOrganisationResponse Invoke(GetOrganisationRequest request)
        {
            Trace("Starting...");

            var organisationId = Organisation.GetId(request.OrganisationId);
            var organisation = Load<Organisation>(organisationId);

            if(organisation != null)
            {
                var paymentPlans = _getPaymentPlansQuery.Invoke(new GetPaymentPlansRequest()).Plans;
                organisation.PaymentPlan = paymentPlans.First(p => p.Id == organisation.PaymentPlanId);
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
