using System.Collections.Generic;
using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using ProtoBuf;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Notifications.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetNotificationsQuery : SessionAccessBase, IGetNotificationsQuery
    {
        public GetNotificationsResponse Invoke(GetNotificationsRequest request)
        {
            Trace("Starting...");

            var notifications = Session.Raven.Query<Notification>();

            return new GetNotificationsResponse
            {
                Notifications = notifications.ToList()
            };
        }
    }

    public interface IGetNotificationsQuery : IQuery<GetNotificationsRequest, GetNotificationsResponse>
    { }

    [ProtoContract]
    public class GetNotificationsResponse
    {
        [ProtoMember(1)]
        public List<Notification> Notifications { get; set; }
    }

    public class GetNotificationsRequest : CacheableRequestBase<GetNotificationsResponse>
    {
        protected override string GetCacheKey()
        {
            return "notifications";
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.System;
        }
    }
}
