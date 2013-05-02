using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Net.Http;
using Errordite.Core.Auditing.Entities;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Master;
using Errordite.Core.Messaging;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Raven;
using Errordite.Core.Session.Actions;
using Errordite.Core.Web;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Indexes;
using Raven.Client.Extensions;
using Errordite.Core.Extensions;

namespace Errordite.Core.Session
{
	public class AppSession : IAppSession
    {
        private IDocumentSession _session;
        private readonly object _syncLock = new object();
        private readonly IShardedRavenDocumentStoreFactory _documentStoreFactory;
        private readonly IComponentAuditor _auditor;
        private List<SessionCommitAction> _sessionCommitActions;
        private HttpClient _receiveServiceHttpClient;
        private IDocumentSession _organisationSession;
        private Organisation _organisation;
        private readonly IMessageSender _messageSender;

        public AppSession(IShardedRavenDocumentStoreFactory documentStoreFactory, 
            IComponentAuditor auditor,
            IMessageSender messageSender)
        {
            _sessionCommitActions = new List<SessionCommitAction>();
            _documentStoreFactory = documentStoreFactory;
            _auditor = auditor;
            _messageSender = messageSender;
        }

        public HttpClient ReceiveHttpClient
        {
            get
            {
				if (_receiveServiceHttpClient == null)
					InitialiseReceiveServiceClient();

	            return _receiveServiceHttpClient;
            }
        }

        public string OrganisationDatabaseName
        {
            get { return _organisation == null ? null : _organisation.FriendlyId; }
        }

        public IDocumentSession MasterRaven
        {
            get
            {
                if(_session != null)
                    return _session;

                _session = _documentStoreFactory.Create(RavenInstance.Master()).OpenSession(CoreConstants.ErrorditeMasterDatabaseName);
				_session.Advanced.MaxNumberOfRequestsPerSession = 500;
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

				if (_organisation == null)
                    throw new InvalidOperationException("Can't get session until the session Organisation has been set.");

				_organisationSession = _documentStoreFactory.Create(_organisation.RavenInstance).OpenSession(_organisation.FriendlyId);
                _organisationSession.Advanced.MaxNumberOfRequestsPerSession = 500;
                _organisationSession.Advanced.UseOptimisticConcurrency = true;
                return _organisationSession;
            }
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
			if (!allowReset && _organisation != null && _organisation.FriendlyId != organisation.FriendlyId)
                throw new InvalidOperationException("Cannot set Organisation twice in one session.");

			_organisation = organisation;
        }

		private void InitialiseReceiveServiceClient()
		{
			if(_organisation == null)
				throw new InvalidOperationException("Can't get receive service http client until the session Organisation has been set.");

			_receiveServiceHttpClient = new HttpClient(new LoggingHttpMessageHandler(_auditor))
			{
				BaseAddress = new Uri("{0}:800/api/{1}/".FormatWith(_organisation.RavenInstance.ServicesBaseUrl, _organisation.FriendlyId))
			};
		}

        public void BootstrapOrganisation(Organisation organisation)
        {
			MasterRavenDatabaseCommands.EnsureDatabaseExists(_organisation.FriendlyId);

            var docStore = _documentStoreFactory.Create(organisation.RavenInstance);

            IndexCreation.CreateIndexes(
                new CompositionContainer(new AssemblyCatalog(typeof(Indexing.Issues).Assembly), new ExportProvider[0]),
                 RavenDatabaseCommands, docStore.Conventions);

			_organisationSession = docStore.OpenSession(_organisation.FriendlyId);
            _organisationSession.Advanced.MaxNumberOfRequestsPerSession = 500;

            var facets = new List<Facet>
            {
                new Facet {Name = "Status"},
            };

            _organisationSession.Store(new FacetSetup { Id = CoreConstants.FacetDocuments.IssueStatus, Facets = facets });
            _organisationSession.SaveChanges();
        }

		public void Close()
		{
			if (_session != null)
			{
				_session.Dispose();
				_session = null;
			}

			if (_organisationSession != null)
			{
				_organisationSession.Dispose();
				_organisationSession = null;
			}

			//trouble with this is there may be an async request in progress when the session closes
			//if (_receiveServiceHttpClient != null)
			//{
			//	_receiveServiceHttpClient.Dispose();
			//	_receiveServiceHttpClient = null;
			//}	
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

		public void Dispose()
		{
			Close();
		}

		#region Switch Organisation

        public IDisposable SwitchOrg(Organisation organisation)
        {
            return new SwitchOrgBack(this, organisation);
        }

        private class SwitchOrgBack : IDisposable
        {
            private readonly IDocumentSession _oldSession;
            private readonly AppSession _appSession;
			private readonly Organisation _oldOrganisation;
			private readonly List<SessionCommitAction> _oldSessionCommitActions;

            public SwitchOrgBack(AppSession appSession, Organisation tempOrg)
            {
                _oldSession = appSession._organisationSession;
				_oldOrganisation = appSession._organisation;
	            _oldSessionCommitActions = appSession._sessionCommitActions;

				_appSession = appSession;
				_appSession._organisationSession = null;
				_appSession._organisation = tempOrg;
				_appSession._sessionCommitActions = new List<SessionCommitAction>();
            }

            public void Dispose()
            {
                _appSession._organisationSession.SaveChanges();
				_appSession._organisationSession.Dispose();

                _appSession._organisationSession = _oldSession;
                _appSession._organisation = _oldOrganisation;
				_appSession._sessionCommitActions = _oldSessionCommitActions;
            }
        }

		#endregion

		#region Sync Indexes

        public void SynchroniseIndexes<T>(bool masterRaven = false)
            where T : AbstractIndexCreationTask, new()
        {
            AddCommitAction(new SynchroniseIndexCommitAction<T>(masterRaven));
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

		#endregion
    }
}
