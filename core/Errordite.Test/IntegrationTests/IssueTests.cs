
using System;
using System.Linq;
using System.Reflection;
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using NUnit.Framework;
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
        }
    }
}
