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
    public class Organisations_Search : AbstractIndexCreationTask<Organisation>
    {
        public Organisations_Search()
        {
            Map = organisations => from o in organisations
                            select new
                            {
                                o.Id,
                                o.Name
                            };

            Analyzers = new Dictionary<Expression<Func<Organisation, object>>, string>
            {
                { e => e.Id, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Name, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };

            Stores = new Dictionary<Expression<Func<Organisation, object>>, FieldStorage>
            {
                {e => e.Id, FieldStorage.No},
                {e => e.Name, FieldStorage.No}
            };
        }
    }
}