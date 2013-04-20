using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Net.Http;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Messaging;
using Errordite.Core.Redis;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Raven;
using Errordite.Core.Web;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Indexes;
using Raven.Client.Extensions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session
{
    public interface IAppSession : IDisposable
    {
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
        /// <summary>
        /// publisher messages
        /// </summary>
        IMessageSender MessageSender { get; }

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
        /// <param name="allowReset"></param>
        void SetOrganisation(Organisation organisation, bool allowReset = false);
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

        /// <summary>
        /// For use when you temporarily want read-only access to an org db other than your own.
        /// Note this only switches the db, not any other per-org things (e.g. reception service web api endpoint)
        /// </summary>
        IDisposable SwitchOrg(Organisation organisation);
    }

    public class AppSession : IAppSession
    {
        private IDocumentSession _session;
        private readonly object _syncLock = new object();
        private readonly IShardedRavenDocumentStoreFactory _documentStoreFactory;
        private readonly IRedisSession _redisSession;
        private readonly IComponentAuditor _auditor;
        private readonly List<SessionCommitAction> _sessionCommitActions;
        private HttpClient _receptionServiceHttpClient;
        private string _organisationDatabaseId;
        private IDocumentSession _organisationSession;
        private RavenInstance _organisationRavenInstance;
        private readonly IMessageSender _messageSender;

        public AppSession(IShardedRavenDocumentStoreFactory documentStoreFactory, 
            IRedisSession redisSession, 
            IComponentAuditor auditor,
            IMessageSender messageSender)
        {
            _sessionCommitActions = new List<SessionCommitAction>();
            _documentStoreFactory = documentStoreFactory;
            _redisSession = redisSession;
            _auditor = auditor;
            _messageSender = messageSender;
        }

        public int RequestLimit { get; set; }

        public HttpClient ReceptionServiceHttpClient
        {
            //TODO: think about disposing + also creating derived class (and so add more specific methods)
            get { return _receptionServiceHttpClient; }
        }

        public string OrganisationDatabaseName
        {
            get { return _organisationDatabaseId; }
        }

        public IDocumentSession MasterRaven
        {
            get
            {
                if(_session != null)
                    return _session;

                _session = _documentStoreFactory.Create(RavenInstance.Master()).OpenSession(CoreConstants.ErrorditeMasterDatabaseName);
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

                _organisationSession = _documentStoreFactory.Create(_organisationRavenInstance).OpenSession(_organisationDatabaseId);
                _organisationSession.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 500 : RequestLimit;
                _organisationSession.Advanced.UseOptimisticConcurrency = true;
                return _organisationSession;
            }
        }

        public IRedisSession Redis
        {
            get { return _redisSession; }
        }

        public IMessageSender MessageSender
        {
            get { return _messageSender; }
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

        public void AddCommitAction(SessionCommitAction sessionCommitAction)
        {
            _sessionCommitActions.Add(sessionCommitAction);
        }

        public void ClearCommitActions()
        {
            _sessionCommitActions.Clear();
        }

        public void SetOrganisation(Organisation organisation, bool allowReset = false /*some system tasks require us to do this*/)
        {
            if (!allowReset && _organisationDatabaseId != null && _organisationDatabaseId != IdHelper.GetFriendlyId(organisation.OrganisationId))
                throw new InvalidOperationException("Cannot set Organisation twice in one session.");

            SetOrganisationContext(organisation);

            var uriBuilder = new UriBuilder(organisation.RavenInstance.ReceptionHttpEndpoint);

            if (!uriBuilder.Path.EndsWith("/"))
                uriBuilder.Path += "/";

            uriBuilder.Path += "{0}/".FormatWith(organisation.FriendlyId);

            _receptionServiceHttpClient = new HttpClient(new LoggingHttpMessageHandler(_auditor)) { BaseAddress = uriBuilder.Uri };
        }

        public void Close()
        {
            if (_session != null)
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

        private void SetOrganisationContext(Organisation organisation)
        {
            if (_organisationSession == null)
            {
                _organisationDatabaseId = IdHelper.GetFriendlyId(organisation.OrganisationId);
                _organisationRavenInstance = organisation.RavenInstance;
                _auditor.Trace(GetType(), "Set organisation id to {0}", _organisationDatabaseId);    
            }
        }

        public void BootstrapOrganisation(Organisation organisation)
        {
			MasterRavenDatabaseCommands.EnsureDatabaseExists(_organisationDatabaseId);

            var docStore = _documentStoreFactory.Create(organisation.RavenInstance);

            IndexCreation.CreateIndexes(
                new CompositionContainer(new AssemblyCatalog(typeof(Issues_Search).Assembly), new ExportProvider[0]),
                 RavenDatabaseCommands, docStore.Conventions);

            _organisationSession = docStore.OpenSession(_organisationDatabaseId);
            _organisationSession.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 250 : RequestLimit;

            var facets = new List<Facet>
            {
                new Facet {Name = "Status"},
            };

            _organisationSession.Store(new FacetSetup { Id = Core.CoreConstants.FacetDocuments.IssueStatus, Facets = facets });
            _organisationSession.SaveChanges();
        }

        public IDisposable SwitchOrg(Organisation organisation)
        {
            return new SwitchOrgBack(this, organisation);
        }

        private class SwitchOrgBack : IDisposable
        {
            private readonly IDocumentSession _oldSession;
            private readonly string _oldDbId;
            private readonly AppSession _appSession;
            private readonly RavenInstance _oldRavenInstance;

            public SwitchOrgBack(AppSession appSession, Organisation tempOrg)
            {
                _oldSession = appSession._organisationSession;
                _oldDbId = appSession._organisationDatabaseId;
                _oldRavenInstance = appSession._organisationRavenInstance;

                appSession._organisationSession = null;
                appSession._organisationDatabaseId = null;
                appSession._organisationRavenInstance = null;

                appSession.SetOrganisationContext(tempOrg);
                _appSession = appSession;
            }

            public void Dispose()
            {
                //temp session does not get committed - just disposed.  If / when we need r/w access we'll have to change this.
                if (_appSession._organisationSession != null)
                    _appSession._organisationSession.Dispose();

                _appSession._organisationSession = _oldSession;
                _appSession._organisationDatabaseId = _oldDbId;
                _appSession._organisationRavenInstance = _oldRavenInstance;
            }
        }

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

        public void Dispose()
        {
            Close();
        }
    }
}
