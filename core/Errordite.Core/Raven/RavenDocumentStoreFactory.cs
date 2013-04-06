using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Raven
{
    public interface IRavenDocumentStoreFactory
    {
        IDocumentStore Create();
    }

    public class RavenDocumentStoreFactory : IRavenDocumentStoreFactory
    {
        private static readonly Guid _resourceManagerId = Guid.NewGuid();

        public IDocumentStore Create()
        {
            var store = new DocumentStore
            {
                Conventions =
                {
                    CustomizeJsonSerializer = ser => ser.TypeNameHandling = TypeNameHandling.All,
                },
                ConnectionStringName = "RavenDB",
                ResourceManagerId = _resourceManagerId,
                EnlistInDistributedTransactions = false,
            }
            .Initialize();
            return store;
        }
    }
}