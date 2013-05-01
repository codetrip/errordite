using System;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;

namespace Errordite.Tasks.Tasks
{
	public class TrialExpirationEmailSender : ComponentBase, ITask
	{
		private readonly IAppSession _session;
		private readonly ErrorditeConfiguration _configuration;
	    private readonly ISuspendOrganisationCommand _suspendOrganisationCommand;

		public TrialExpirationEmailSender(IAppSession session, 
			ErrorditeConfiguration configuration, 
            ISuspendOrganisationCommand suspendOrganisationCommand)
		{
			_session = session;
			_configuration = configuration;
		    _suspendOrganisationCommand = suspendOrganisationCommand;
		}

		public void Execute(string ravenInstanceId)
		{
            var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
                                        .Where(o => o.CurrentPeriodEndDate >= DateTime.UtcNow.Date)
                                        .Where(o => o.SubscriptionStatus == SubscriptionStatus.Cancelled && o.OrganisationStatus == OrganisationStatus.Active)
			                            .As<Organisation>()
			                            .ToList();

			if (organisations.Any())
			{
				Trace("Found {0} organisations requiring cancellation", organisations.Count);

				foreach (var organisation in organisations)
				{
				    _suspendOrganisationCommand.Invoke(new SuspendOrganisationRequest
				    {
				        Reason = SuspendedReason.SubscriptionCancelled,
				        OrganisationId = organisation.Id
				    });
				}
			}
		}
	}
}
