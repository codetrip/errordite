using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Errordite.Core.Domain.Error;
using Lucene.Net.Analysis;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Errordite.Core.Indexing
{
    public class ErrorDocument
    {
        public string Query { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string ApplicationId { get; set; }
        public string IssueId { get; set; }
        public string Id { get; set; }
        public int FriendlyId { get; set; }
    }

    public class Errors : AbstractIndexCreationTask<Error, ErrorDocument>
    {
        public Errors()
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
                                    error.Version,
                                    //as a further "woo", it is recursive, so we can search on properties of any of the ExceptionInfos (i.e the inner ones)
                                    error.ExceptionInfos.Select(i => new object[]
                                    { 
                                        i.Type,
                                        i.Message,
                                        i.MethodName,
                                        i.Module,
                                        i.StackTrace,
                                        i.ExtraData.Select(x => x.Value),
                                    })
                                },
                                error.TimestampUtc,
                                error.ApplicationId,
                                error.IssueId,
                                error.Id,
                                FriendlyId = int.Parse(error.Id.Split('/')[1])
                            };

            Indexes = new Dictionary<Expression<Func<ErrorDocument, object>>, FieldIndexing>
                {
                    {e => e.Query, FieldIndexing.Analyzed},
                };

            Analyzers = new Dictionary<Expression<Func<ErrorDocument, object>>, string>
                {
                    {e => e.Query, typeof(SimpleAnalyzer).FullName}, //SimpleAnalyzer tokenizes on all non-alphanumeric characters
                };

            Sort(e => e.TimestampUtc, SortOptions.String);
            Sort(e => e.FriendlyId, SortOptions.Int);
        }
    }
}