using System;
using System.Collections.Generic;
using CodeTrip.Core.Interfaces;
using CodeTrip.Core.Session;
using System.Linq;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Alerts
{
    public class GetUserAlertsQuery : SessionAccessBase, IGetUserAlertsQuery
    {
        public GetUserAlertsResponse Invoke(GetUserAlertsRequest request)
        {
            var alerts = Session.Raven.Query<UserAlert>().Where(a => a.UserId == request.UserId);

            if (request.NewerThanUtc.HasValue)
                alerts = alerts.Where(a => a.SentUtc > request.NewerThanUtc.Value);

            return new GetUserAlertsResponse { Alerts = alerts };
        }
    }

    public interface IGetUserAlertsQuery : IQuery<GetUserAlertsRequest, GetUserAlertsResponse>
    { }

    public class GetUserAlertsRequest
    {
        public string UserId { get; set; }
        public DateTime? NewerThanUtc { get; set; }
    }

    public class GetUserAlertsResponse
    {
        public IEnumerable<UserAlert> Alerts { get; set; }
    }
}