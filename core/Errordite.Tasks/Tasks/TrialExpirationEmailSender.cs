
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using Raven.Client.Linq;

namespace Errordite.Tasks.Tasks
{
	public class TrialExpirationEmailSender : ITask
	{
		private readonly IAppSession _session;

		public TrialExpirationEmailSender(IAppSession session)
		{
			_session = session;
		}

		public void Execute(string ravenInstanceId)
		{
			var organisations = _session.MasterRaven.Query<Organisation, Organisations>()
				.Where(o => o.S)
		}
	}
}
