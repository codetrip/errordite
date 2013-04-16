using System;
using System.Collections.Generic;
using Errordite.Core.Domain.Central;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Raven
{
    public interface IShardedRavenDocumentStoreFactory
    {
        IDocumentStore Create(RavenInstance instance);
    }

    public class ShardedRavenDocumentStoreFactory : IShardedRavenDocumentStoreFactory
    {
        private static readonly Guid _resourceManagerId = Guid.NewGuid();
        private static readonly Dictionary<string, IDocumentStore> _documentStores = new Dictionary<string, IDocumentStore>();
        private readonly object _syncLock = new object();

        public IDocumentStore Create(RavenInstance instance)
        {
            if (!_documentStores.ContainsKey(instance.Id))
            {
                lock (_syncLock)
                {
                    if (!_documentStores.ContainsKey(instance.Id))
                    {
                        var store = new DocumentStore
                        {
                            Conventions =
                            {
                                CustomizeJsonSerializer = ser => ser.TypeNameHandling = TypeNameHandling.All,
                            },
                            ResourceManagerId = _resourceManagerId,
                            EnlistInDistributedTransactions = false,
                        };

                        if (instance.IsMaster)
                            store.ConnectionStringName = "RavenDB";
                        else
                            store.Url = instance.RavenUrl;

                        store.Initialize();
                        _documentStores.Add(instance.Id, store);
                    }
                }
            }

            return _documentStores[instance.Id];
        }
    }
}