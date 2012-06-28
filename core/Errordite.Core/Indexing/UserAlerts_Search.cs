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

            Analyzers = new Dictionary<Expression<Func<UserAlert, object>>, string>
            {
                { e => e.UserId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.SentUtc, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };

            Stores = new Dictionary<Expression<Func<UserAlert, object>>, FieldStorage>
            {
                {e => e.UserId, FieldStorage.No},
                {e => e.SentUtc, FieldStorage.No}
            };

            Sort(e => e.SentUtc, SortOptions.String);
        }
    }
}