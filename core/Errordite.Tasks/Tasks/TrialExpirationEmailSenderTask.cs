using System;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Session;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;

namespace Errordite.Tasks.Tasks
{
	public class TrialExpirationEmailSenderTask : ComponentBase, ITask
	{
		private readonly IAppSession _session;
		private readonly ErrorditeConfiguration _configuration;

		public TrialExpirationEmailSenderTask(IAppSession session, 
			ErrorditeConfiguration configuration)
		{
			_session = session;
			_configuration = configuration;
		}

		public void Execute(string ravenInstanceId)
		{
			var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
			                            .Where(o => o.CreatedOnDate == DateTime.UtcNow.AddDays(-_configuration.TrialLengthInDays).Date)
			                            .Where(o => o.SubscriptionStatus == SubscriptionStatus.Trial)
			                            .As<Organisation>()
			                            .ToList();

			if (organisations.Any())
			{
				Trace("Found {0} organisations with trials expiring today", organisations.Count);

				foreach (var organisation in organisations)
				{
                    using (_session.SwitchOrg(organisation))
                    {
                        var primaryUser = _session.Raven.Query<User>().FirstOrDefault(m => m.Id == organisation.PrimaryUserId);

                        if (primaryUser != null)
                        {
                            _session.MessageSender.Send(new TrialExpiredEmailInfo
                            {
                                OrganisationName = organisation.Name,
                                To = primaryUser.Email,
                                UserName = primaryUser.FirstName
                            },
                            _configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId));
                        }
                    }
				}
			}
		}
	}
}
