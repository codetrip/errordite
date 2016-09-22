
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace Errordite.Test.Raven
{
    [TestFixture]
    public class RavenTestBase
    {
        protected EmbeddableDocumentStore DocumentStore { get; private set; }

        [SetUp]
        protected void Setup()
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                DataDirectory = "Data"
            };
            DocumentStore.Initialize();
        }

        [TearDown]
        protected void TearDown()
        {
            DocumentStore.Dispose();
        }

        protected IDocumentSession GetSession()
        {
            return DocumentStore.OpenSession();
        }
    }
}
