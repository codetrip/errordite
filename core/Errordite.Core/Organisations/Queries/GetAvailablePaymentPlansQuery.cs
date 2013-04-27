using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using ProtoBuf;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetAvailablePaymentPlansQuery : SessionAccessBase, IGetAvailablePaymentPlansQuery
    {
        public GetAvailablePaymentPlansResponse Invoke(GetAvailablePaymentPlansRequest request)
        {
            Trace("Starting...");

            var plans = Session.MasterRaven.Query<PaymentPlan>()
                .Where(p => p.IsAvailable)
                .ToList();

            Trace("Found {0} Payment Plans.", plans.Count);

            return new GetAvailablePaymentPlansResponse
            {
                Plans = plans
            };
        }
    }

    public interface IGetAvailablePaymentPlansQuery : IQuery<GetAvailablePaymentPlansRequest, GetAvailablePaymentPlansResponse>
    { }

    [ProtoContract]
    public class GetAvailablePaymentPlansResponse
    {
        [ProtoMember(1)]
        public List<PaymentPlan> Plans { get; set; }
    }

    public class GetAvailablePaymentPlansRequest : CacheableRequestBase<GetAvailablePaymentPlansResponse>
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
