using Errordite.Core.Issues.Commands;
using Errordite.Core.Messages;
using Errordite.Core.ServiceBus;

namespace Errordite.Events.Service.Handlers
{
    public class CheckIssueHandler : MessageHandlerSessionBase<CheckIssueMessage>
    {
        private readonly ICheckIssueCommand _checkIssueCommand;

        public CheckIssueHandler(ICheckIssueCommand checkIssueCommand)
        {
            _checkIssueCommand = checkIssueCommand;
        }

        protected override void HandleMessage(CheckIssueMessage message)
        {
            _checkIssueCommand.Invoke(new CheckIssueRequest
            {
                IssueId = message.IssueId,
            });
        }
    }
}