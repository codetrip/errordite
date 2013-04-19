
using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Central;
using Errordite.Core.Session;
using ProtoBuf;
using System.Linq;
using Raven.Client;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetRavenInstancesQuery : SessionAccessBase, IGetRavenInstancesQuery
    {
        public GetRavenInstancesResponse Invoke(GetRavenInstancesRequest request)
        {
            Trace("Starting...");

            if (Session == null)
            {
                return new GetRavenInstancesResponse
                {
                    RavenInstances = request.Session.Query<RavenInstance>().ToList()
                };
            }

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
        public IDocumentSession Session { get; set; }

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
