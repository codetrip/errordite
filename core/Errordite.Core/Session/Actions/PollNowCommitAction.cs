using System;
using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Web;

namespace Errordite.Core.Session.Actions
{
	public class PollNowCommitAction : SessionCommitAction
	{
		private readonly object _syncLock = new object();
		private readonly Organisation _organisation;
		private static readonly IDictionary<string, DateTime> _organisationLastPoll = new Dictionary<string, DateTime>();

		public PollNowCommitAction(Organisation organisation)
		{
			_organisation = organisation;
		}

		public override void Execute(IAppSession session)
		{
			if(_organisationLastPoll.ContainsKey(_organisation.FriendlyId))
			{
				var lastPoll = _organisationLastPoll[_organisation.FriendlyId];
				_organisationLastPoll[_organisation.FriendlyId] = DateTime.UtcNow;

				//changed this to poll a max of every 20 seconds as there is no point polling more
				//frequently because of the long polling of the SQS queue
				if (DateTime.UtcNow - lastPoll >= TimeSpan.FromSeconds(20))
				{
					PollNow(session);
				}
			}
			else
			{
				lock (_syncLock)
				{
					if (!_organisationLastPoll.ContainsKey(_organisation.FriendlyId))
					{
						_organisationLastPoll.Add(_organisation.FriendlyId, DateTime.UtcNow);
						PollNow(session);
					}
				}
			}
		}

		private void PollNow(IAppSession session)
		{
			session.ReceiveHttpClient.PostJsonAsync("pollnow", new { organisationFriendlyId = _organisation.FriendlyId });
		}
	}
}