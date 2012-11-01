using System.Linq;
using Errordite.Core.Domain.Organisation;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class UserAlerts_Search : AbstractIndexCreationTask<UserAlert>
    {
        public UserAlerts_Search()
        {
            Map = alerts => from alert in alerts
                            select new
                            {
                                alert.UserId,
                                alert.SentUtc
                            };
        }
    }
}