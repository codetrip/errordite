using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace CodeTrip.Core.RavenDb
{
    public interface IRavenDocumentStoreFactory
    {
        IDocumentStore Create();
    }

    public class RavenDocumentStoreFactory : IRavenDocumentStoreFactory
    {
        private static readonly Guid _resourceManagerId = Guid.NewGuid();
        private readonly RavenConfiguration _ravenConfiguration;

        public RavenDocumentStoreFactory(RavenConfiguration ravenConfiguration)
        {
            _ravenConfiguration = ravenConfiguration;
        }

        public IDocumentStore Create()
        {
            var store = new DocumentStore
            {
                Url = _ravenConfiguration.Endpoint,
                ResourceManagerId = _resourceManagerId,
                Conventions =
                {
                    CustomizeJsonSerializer = ser => ser.TypeNameHandling = TypeNameHandling.All
                },
            }
            //.RegisterListener(new AddProdProfInfoListener())
            .Initialize();
            return store;
        }
    }
}