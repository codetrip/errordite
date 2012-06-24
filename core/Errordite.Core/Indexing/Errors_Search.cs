using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Error;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class ErrorDocument
    {
        public string Query { get; set; }
        public DateTime TimestampUtc { get; set; }
        public bool Classified { get; set; }
        public string OrganisationId { get; set; }
        public string ApplicationId { get; set; }
        public string IssueId { get; set; }
        public string Id { get; set; }
    }

    public class Errors_Search : AbstractIndexCreationTask<Error, ErrorDocument>
    {
        public Errors_Search()
        {
            Map = errors => from error in errors
                            select new
                            {
                                //Raven maps IEnumerables to multi-value Lucene fields, meaning you can just whack as much 
                                //stuff into a query field as you like and it'll match on any of them - woo!  
                                //see http://ayende.com/blog/152833/orders-search-in-ravendb
                                Query = new object[]
                                {
                                    error.Url,
                                    error.MachineName,
                                    error.IssueId,
                                    //as a further "woo", it is recursive, so we can search on properties of any of the ExceptionInfos (i.e the inner ones)
                                    error.ExceptionInfos.Select(i => new object[]
                                    { 
                                        i.Type,
                                        i.Message,
                                        i.MethodName,
                                        i.Module,
                                        i.StackTrace,
                                    })
                                },
                                error.TimestampUtc,
                                error.Classified,
                                error.OrganisationId,
                                error.ApplicationId,
                                error.IssueId,
                                error.Id
                            };

            Analyzers = new Dictionary<Expression<Func<ErrorDocument, object>>, string>
            {
                { e => e.Query, typeof(StandardAnalyzer).AssemblyQualifiedName},
                { e => e.IssueId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Classified, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.OrganisationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.ApplicationId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.IssueId, typeof(KeywordAnalyzer).AssemblyQualifiedName },
                { e => e.Id, typeof(KeywordAnalyzer).AssemblyQualifiedName }
            };

            Stores = new Dictionary<Expression<Func<ErrorDocument, object>>, FieldStorage>
            {
                {e => e.Query, FieldStorage.No},
                {e => e.TimestampUtc, FieldStorage.No},
                {e => e.Classified, FieldStorage.No},
                {e => e.OrganisationId, FieldStorage.No},
                {e => e.ApplicationId, FieldStorage.No},
                {e => e.IssueId, FieldStorage.No},
                {e => e.Id, FieldStorage.No}
            };

            Sort(e => e.TimestampUtc, SortOptions.String);
        }
    }
}