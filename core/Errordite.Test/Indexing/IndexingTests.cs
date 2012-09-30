
using Errordite.Core;
using Errordite.Core.Domain.Error;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using System.Linq;

namespace Errordite.Test.Indexing
{
    [TestFixture]
    public class IndexingTests : ErrorditeTestBase
    {
        [Test]
        public void UpdateLastItem()
        {
            var errors = Session.Raven.Advanced.DocumentStore.DatabaseCommands.Query("Errors/Search", new IndexQuery { Query = "IssueId:issues/105" }, new[] { "Classified", "IssueId" });

            Assert.That(errors.Results != null);
            Assert.That(errors.Results.Count > 0);

            RollbackTransaction = false;

            Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex("Errors/Search",
                    new IndexQuery
                    {
                        Query = "IssueId:issues/105 AND Classified:false"
                    },
                    new[]
                    {
                        new PatchRequest
                        {
                            Name = "Classified",
                            Type = PatchCommandType.Set,
                            Value = true
                        }
                    }, true);

            Session.Commit();
        }

        [Test]
        public void PatchMultivaluedFieldTest()
        {
            Session.Raven.Advanced.DocumentStore.DatabaseCommands.UpdateByIndex(CoreConstants.IndexNames.Errors,
                new IndexQuery
                {
                    Query = "Id: errors/37"
                },
                new[]
                {
                    new PatchRequest
                    {
                        Name = "IssueId",
                        Type = PatchCommandType.Set,
                        Value = "9"
                    }
                });

            Session.Commit();
        }

        [Test]
        public void Test()
        {
            var error = Session.Raven.Query<Error>().First();
        }
    }
}
