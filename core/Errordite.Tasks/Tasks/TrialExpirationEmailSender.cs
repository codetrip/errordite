﻿using System;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Master;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.EmailInfo;
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

		public TrialExpirationEmailSender(IAppSession session, 
			ErrorditeConfiguration configuration)
		{
			_session = session;
			_configuration = configuration;
		}

		public void Execute(string ravenInstanceId)
		{
			var organisations = _session.MasterRaven.Query<OrganisationDocument, Organisations>()
			                            .Where(o => o.CreatedOnUtc == DateTime.UtcNow.AddDays(-_configuration.TrialLengthInDays).Date)
			                            .Where(o => o.SubscriptionStatus == SubscriptionStatus.Trial)
			                            .As<Organisation>()
			                            .ToList();

			if (organisations.Any())
			{
				Trace("Found {0} organisations with trials expiring today", organisations.Count);

				foreach (var organisation in organisations)
				{
					var users = _session.MasterRaven.Query<UserOrganisationMapping>().Where(m => m.OrganisationId == organisation.Id);

					_session.MessageSender.Send(new TrialExpiredEmailInfo
					{
						OrganisationName = organisation.Name,
						To = string.Join(",", users.Select(u => u.EmailAddress))
					},
					_configuration.GetNotificationsQueueAddress(organisation.RavenInstanceId));
				}
			}
		}
	}
}