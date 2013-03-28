
using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Central;
using Errordite.Core.Session;
using ProtoBuf;
using System.Linq;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetRavenInstancesQuery : SessionAccessBase, IGetRavenInstancesQuery
    {
        public GetRavenInstancesResponse Invoke(GetRavenInstancesRequest request)
        {
            Trace("Starting...");

            return new GetRavenInstancesResponse
            {
                RavenInstances = Session.MasterRaven.Query<RavenInstance>().ToList()
            };
        }
    }

    public interface IGetRavenInstancesQuery : IQuery<GetRavenInstancesRequest, GetRavenInstancesResponse>
    { }

    [ProtoContract]
    public class GetRavenInstancesResponse
    {
        [ProtoMember(1)]
        public List<RavenInstance> RavenInstances { get; set; }
    }

    public class GetRavenInstancesRequest : CacheableRequestBase<GetRavenInstancesResponse>
    {
        protected override string GetCacheKey()
        {
            return "errordite-instances";
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.System;
        }
    }
}
