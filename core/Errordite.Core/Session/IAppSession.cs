using System;
using System.Net.Http;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Messaging;
using Errordite.Core.Session.Actions;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Indexes;

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
		/// Http interface for the reception services
		/// </summary>
		HttpClient ReceiveHttpClient { get; }

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
}