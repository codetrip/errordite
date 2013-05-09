using System;
using Errordite.Core;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;

namespace Errordite.Tasks.Tasks
{
	public class DeleteSuspendedOrganisationsTask : ComponentBase, ITask
	{
		private readonly IAppSession _session;
	    private readonly IDeleteOrganisationCommand _deleteOrganisationCommand;

		public DeleteSuspendedOrganisationsTask(IAppSession session, IDeleteOrganisationCommand deleteOrganisationCommand)
		{
			_session = session;
			_deleteOrganisationCommand = deleteOrganisationCommand;
		}

		public void Execute(string ravenInstanceId)
		{
			var date = DateTime.UtcNow.AddDays(-15).Date;
            var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
                                        .Where(o => o.SuspendedOnUtc <= date)
                                        .Where(o => o.OrganisationStatus == OrganisationStatus.Suspended)
			                            .As<Organisation>()
			                            .ToList();

			if (organisations.Any())
			{
				Trace("Found {0} organisations requiring cancellation", organisations.Count);

				foreach (var organisation in organisations)
				{
					_deleteOrganisationCommand.Invoke(new DeleteOrganisationRequest
					{
						OrganisationId = organisation.Id
					});
				}
			}
		}
	}
}
