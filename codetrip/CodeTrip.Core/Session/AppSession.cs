using System.Collections.Generic;
using CodeTrip.Core.Redis;
using NServiceBus;
using Raven.Client;
using Raven.Client.Indexes;

namespace CodeTrip.Core.Session
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
        IDocumentSession Raven { get; }
        /// <summary>
        /// Access the Redis Databases
        /// </summary>
        IRedisSession Redis { get; }
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
        /// Synchronise the index specified in the type parameter
        /// </summary>
        void SynchroniseIndexes<T>() 
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
        private readonly List<SessionCommitAction> _sessionCommitActions;

        public AppSession(IDocumentStore documentStore, IBus bus, IRedisSession redisSession)
        {
            _sessionCommitActions = new List<SessionCommitAction>();
            _documentStore = documentStore;
            _bus = bus;
            _redisSession = redisSession;
        }

        public IBus Bus
        {
            get { return _bus; }
        }

        public IDocumentSession Raven
        {
            get
            {
                if(_session != null)
                    return _session;

                _session = _documentStore.OpenSession();
                _session.Advanced.MaxNumberOfRequestsPerSession = RequestLimit == 0 ? 50 : RequestLimit;
                return _session;
            }
        }

        public IRedisSession Redis
        {
            get { return _redisSession; }
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
        }

        public void Commit()
        {
            lock (_syncLock)
            {
                if (_session != null)
                    _session.SaveChanges();

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

        public void SynchroniseIndexes<T>()
            where T : AbstractIndexCreationTask, new()
        {
            AddCommitAction(new SynchroniseIndex<T>());
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
    }
}
