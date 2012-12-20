using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Net.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Redis;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.WebApi;
using NServiceBus;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Indexes;
using Raven.Client.Extensions;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Session
{
    public interface IAppSession
    {
        /// <summary>
        /// NServiceBus Bus
        /// </summary>
        IBus Bus { get; }
        /// <summary>
        /// Access the Raven Session
        /// </summary>
        IDocumentSession MasterRaven { get; }
        /// <summary>
        /// Access to organisation dspecific databases
        /// </summary>
        IDocumentSession Raven { get; }
        /// <summary>
        /// Access the Redis Databases
        /// </summary>
        IRedisSession Redis { get; }

        //TODO: make these part of the UoW by adding an action on IDatabaseCommands
	    IDatabaseCommands RavenDatabaseCommands { get; }
	    IDatabaseCommands MasterRavenDatabaseCommands { get; }

        /// <summary>
        /// Disposes and nulls the Raven session, does not perform any other operations, you 
        /// should call Commit() prior to calling Clear() to persist your session.
        /// </summary>
        void Close();
        /// <summary>
        /// Saves changes within the session and executes all session flush actions
        /// </summary>
        void Commit();
        /// <summary>
        /// Add an NServiceBus message to send when committing the session.
        /// </summary>
        /// <param name="sessionCommitAction">Teh session flush action.</param>
        void AddCommitAction(SessionCommitAction sessionCommitAction);
        /// <summary>
        /// Clear down the list of actions to invoke on commit.
        /// </summary>
        void ClearCommitActions();
        /// <summary>
        /// The maximum number of requests per session
        /// </summary>
        int RequestLimit { get; set; }
        /// <summary>
        /// Http interface for the reception services
        /// </summary>
        HttpClient ReceptionServiceHttpClient { get; }
        /// <summary>
        /// Sets the organisationId for this session
        /// </summary>
        /// <param name="organisation"></param>
        void SetOrganisation(Organisation organisation);
        /// <summary>
        /// Gets the organisationId for this session
        /// </summary>
        string OrganisationDatabaseName { get; }

        /// <summary>
        /// Sets up a new organisation in Raven
        /// </summary>
        /// <param name="organisation"></param>
        void BootstrapOrganisation(Organisation organisation);

        /// <summary>
        /// Synchronise the index specified in the type parameter
        /// </summary>
        void SynchroniseIndexes<T>(bool masterRaven = false) 
            where T : AbstractIndexCreationTask, new();

        /// <summary>
        /// Synchronise the index specified in the type parameter
        /// </summary>
        void SynchroniseIndexes<T1, T2>()
            where T1 : AbstractIndexCreationTask, new()
            where T2 : AbstractIndexCreationTask, new();

        /// <summary>
        /// Synchronise the index specified in the type parameter
        /// </summary>
        void SynchroniseIndexes<T1, T2, T3>()
            where T1 : AbstractIndexCreationTask, new()
            where T2 : AbstractIndexCreationTask, new()
            where T3 : AbstractIndexCreationTask, new();
    }

    public class AppSession : IAppSession
    {
        private IDocumentSession _session;
        private readonly object _syncLock = new object();
        private readonly IDocumentStore _documentStore;
        private readonly IBus _bus;
        private readonly IRedisSession _redisSession;
        private readonly ErrorditeConfiguration _config;
        private readonly IComponentAuditor _auditor;
        private readonly List<SessionCommitAction> _sessionCommitActions;
        private HttpClient _receptionServiceHttpClient;
        private string _organisationDatabaseId;
        private IDocumentSession _organisationSession;

        public AppSession(IDocumentStore documentStore, IBus bus, IRedisSession redisSession, ErrorditeConfiguration config, IComponentAuditor auditor)
        {
            _sessionCommitActions = new List<SessionCommitAction>();
            _documentStore = documentStore;
            _bus = bus;     
            _redisSession = redisSession;
            _config = config;
            _auditor = auditor;
        }

        public HttpClient ReceptionServiceHttpClient
        {
            //TODO: think about disposing + also creating derived class (and so add more specific methods)
            get { return _receptionServiceHttpClient; }
        }

        public IBus Bus
        {
            get { return _bus; }
        }

        public IDocumentSession MasterRaven
        {
            get
            {
                if(_session != null)
                    return _session;

                _session = _documentStore.OpenSession(CoreConstants.ErrorditeMasterDatabaseName);
                _session.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 500 : RequestLimit;
                _session.Advanced.UseOptimisticConcurrency = true;
                return _session;
            }
        }

        public IDocumentSession Raven
        {
            get
            {
                if (_organisationSession != null)
                    return _organisationSession;

                if (_organisationDatabaseId == null)
                    throw new InvalidOperationException("Can't get session till Organisation has beens set.");

                _organisationSession = _documentStore.OpenSession(_organisationDatabaseId);
                _organisationSession.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 500 : RequestLimit;
                _organisationSession.Advanced.UseOptimisticConcurrency = true;
                return _organisationSession;
            }
        }

        public IRedisSession Redis
        {
            get { return _redisSession; }
        }

        public IDatabaseCommands RavenDatabaseCommands
        {
			get
			{
				return MasterRaven.Advanced.DocumentStore.DatabaseCommands.ForDatabase(OrganisationDatabaseName);
			}
        }

        public IDatabaseCommands MasterRavenDatabaseCommands
        { 
			get
			{
				return MasterRaven.Advanced.DocumentStore.DatabaseCommands.ForDatabase(CoreConstants.ErrorditeMasterDatabaseName);
			}
        }

        public void Close()
        {
           if(_session != null)
           {
               lock (_syncLock)
               {
                   if (_session != null)
                   {
                       _session.Dispose();
                       _session = null;
                   }
               }
           }

           if (_organisationSession != null)
           {
               lock (_syncLock)
               {
                   if (_organisationSession != null)
                   {
                       _organisationSession.Dispose();
                       _organisationSession = null;
                   }
               }
           }
        }

        public void Commit()
        {
            lock (_syncLock)
            {
                if (_session != null)
                    _session.SaveChanges();
                if (_organisationSession != null)
                    _organisationSession.SaveChanges();

                foreach (var sessionCommitAction in _sessionCommitActions)
                    sessionCommitAction.Execute(this);

                _sessionCommitActions.Clear();
            }
        }

        public void AddCommitAction(SessionCommitAction sessionCommitAction)
        {
            _sessionCommitActions.Add(sessionCommitAction);
        }

        public void ClearCommitActions()
        {
            _sessionCommitActions.Clear();
        }

        public int RequestLimit { get; set; }

        public void SetOrganisation(Organisation organisation)
        {
            if (_organisationDatabaseId != null && _organisationDatabaseId != IdHelper.GetFriendlyId(organisation.OrganisationId))
            {
                throw new InvalidOperationException("Cannot set Organisation twice in one session.");
                //return;
            }

            SetDbId(organisation);

            var uriBuilder = new UriBuilder(_config.ReceptionHttpEndpoint);

            if (!uriBuilder.Path.EndsWith("/"))
                uriBuilder.Path += "/";

            uriBuilder.Path += "{0}/".FormatWith(organisation.FriendlyId);

            _receptionServiceHttpClient = new HttpClient(new LoggingHttpMessageHandler(_auditor)) { BaseAddress = uriBuilder.Uri };
        }

        public string OrganisationDatabaseName { get { return _organisationDatabaseId; } }

        public void SynchroniseIndexes<T>(bool masterRaven = false)
            where T : AbstractIndexCreationTask, new()
        {
            AddCommitAction(new SynchroniseIndex<T>(masterRaven));
        }

        public void SynchroniseIndexes<T1, T2>() 
            where T1 : AbstractIndexCreationTask, new()
            where T2 : AbstractIndexCreationTask, new()
        {
            SynchroniseIndexes<T1>();
            SynchroniseIndexes<T2>();
        }

        public void SynchroniseIndexes<T1, T2, T3>() 
            where T1 : AbstractIndexCreationTask, new()
            where T2 : AbstractIndexCreationTask, new()
            where T3 : AbstractIndexCreationTask, new()
        {
            SynchroniseIndexes<T1>();
            SynchroniseIndexes<T2>();
            SynchroniseIndexes<T3>();
        }

        private void SetDbId(Organisation organisation)
        {
            _organisationDatabaseId = IdHelper.GetFriendlyId(organisation.OrganisationId);
            _auditor.Trace(GetType(), "Set organisation id to {0}", _organisationDatabaseId);
        }

        public void BootstrapOrganisation(Organisation organisation)
        {
            SetDbId(organisation);
			MasterRavenDatabaseCommands.EnsureDatabaseExists(_organisationDatabaseId);

            IndexCreation.CreateIndexes(
                new CompositionContainer(new AssemblyCatalog(typeof(Issues_Search).Assembly), new ExportProvider[0]),
                 RavenDatabaseCommands, _documentStore.Conventions);

            _organisationSession = _documentStore.OpenSession(_organisationDatabaseId);
            _organisationSession.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 250 : RequestLimit;

            var facets = new List<Facet>
            {
                new Facet {Name = "Status"},
            };

            _organisationSession.Store(new FacetSetup { Id = Core.CoreConstants.FacetDocuments.IssueStatus, Facets = facets });
            _organisationSession.SaveChanges();
        }
    }
}
