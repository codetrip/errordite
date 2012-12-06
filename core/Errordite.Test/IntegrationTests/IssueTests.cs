
using System;
using System.Linq;
using System.Reflection;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Paging;
using Errordite.Core;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Issues.Queries;
using Errordite.Core.Session;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace Errordite.Test.IntegrationTests
{
    [TestFixture]
    public class IssueTests : ErrorditeTestBase
    {
        [Test]
        public void GetIssuesViaProjection()
        {
            RollbackTransaction = false;

            var query = Session.Raven.Query<IssueDocument, Issues_Search>()
                .Where(e => e.ApplicationId == "applications/97")
                .As<Issue>()
                .Select(issue => new IssueBase
                {
                    Id = issue.Id,
                    Rules = issue.Rules,
                    LastRuleAdjustmentUtc = issue.LastErrorUtc,
                    MatchPriority = issue.MatchPriority
                })
                .ToList();

            Assert.That(query.Count > 0);
        }

        [Test]
        public void GetIssuesByIdFromLuceneQuery()
        {
            var query = Session.Raven.Advanced.LuceneQuery<Issue>("Issues/Search")
                .Where(new[] { "issues/37", "issues/48", "issues/51" }.ToRavenQuery("Id"))
                .WaitForNonStaleResultsAsOfNow()
                .ToList();

            Assert.That(query != null);
        }

        [Test]
        public void SelectIssuesForPurge()
        {
            var query = Get<IGetIssuesWithNoErrorsWithinPeriodQuery>();

            var issues = query.Invoke(new GetIssuesWithNoErrorsWithinPeriodRequest
            {
                ApplicationId = "1",
                Paging = new PageRequestWithSort(1, 10),
                PurgeDate = DateTime.UtcNow.AddDays(-3)
            }).Issues;

            Assert.That(issues != null);
        }

        [Test]
        public void HourIndex()
        {
            var ravenSession = Get<IAppSession>().Raven;

            var issue = new Issue() {Name = "Test"};

            IndexCreation.CreateIndexes(Assembly.GetAssembly(GetType()), ravenSession.Advanced.DocumentStore);

            ravenSession.Store(issue);
            ravenSession.Store(new Error()
                                   {
                                        IssueId   = issue.Id,
                                        TimestampUtc = new DateTime(2011, 1, 1, 2, 0, 0),
                                   });

            ravenSession.Store(new Error()
            {
                IssueId = issue.Id,
                TimestampUtc = new DateTime(2011, 1, 1, 4, 0, 0),
            });
            ravenSession.SaveChanges();
            

            var result = ravenSession.Query<ByHourReduceResult, Errors_ByIssueByHour>()
                .Customize(x => x.WaitForNonStaleResults())
                .Where(r => r.IssueId == issue.Id)
                .ToArray();




        }

        [Test]
        public void UpdateClassifiedByIndex()
        {
            var session = Get<IAppSession>();

            session.RavenDatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = "ApplicationId:{0} AND IssueIds:{1} AND Classified:false".FormatWith("applications/1", "5123")
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "Classified",
                        Type = PatchCommandType.Set,
                        Value = false
                    }
                });

            Assert.That(true);
        }
    }

}
