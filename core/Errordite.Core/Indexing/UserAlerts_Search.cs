using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Organisation;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
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