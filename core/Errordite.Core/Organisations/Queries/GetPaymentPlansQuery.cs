using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using Errordite.Core.Domain.Organisation;
using ProtoBuf;
using System.Linq;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetPaymentPlansQuery : SessionAccessBase, IGetPaymentPlansQuery
    {
        public GetPaymentPlansResponse Invoke(GetPaymentPlansRequest request)
        {
            Trace("Starting...");

            var plans = Session.Raven.Query<PaymentPlan>().ToList();

            Trace("Found {0} Payment Plans.", plans.Count);

            return new GetPaymentPlansResponse
            {
                Plans = plans
            };
        }
    }

    public interface IGetPaymentPlansQuery : IQuery<GetPaymentPlansRequest, GetPaymentPlansResponse>
    { }

    [ProtoContract]
    public class GetPaymentPlansResponse
    {
        [ProtoMember(1)]
        public List<PaymentPlan> Plans { get; set; }
    }

    public class GetPaymentPlansRequest : CacheableRequestBase<GetPaymentPlansResponse>
    {
        protected override string GetCacheKey()
        {
            return "paymentplans";
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.System;
        }
    }
}
