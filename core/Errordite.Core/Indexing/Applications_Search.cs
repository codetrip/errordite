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
    public class Applications_Search : AbstractIndexCreationTask<Application>
    {
        public Applications_Search()
        {
            Map = applications => 
                from a in applications
                select new
                {
                    a.Id,
                    a.Name,
                    a.OrganisationId,
                    a.Token,
                    a.DefaultUserId
                };

            Analyzers = new Dictionary<Expression<Func<Application, object>>, string>
            {
                { e => e.Id, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Name, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.OrganisationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Token, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.DefaultUserId, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };

            Stores = new Dictionary<Expression<Func<Application, object>>, FieldStorage>
            {
                {e => e.Id, FieldStorage.No},
                {e => e.Name, FieldStorage.No},
                {e => e.OrganisationId, FieldStorage.No},
                {e => e.Token, FieldStorage.No},
                {e => e.DefaultUserId, FieldStorage.No}
            };
        }
    }
}