using Errordite.Core.Issues.Commands;
using Errordite.Core.Messaging;

namespace Errordite.Services.Consumers
{
    public class SyncIssueCountsConsumer : IErrorditeConsumer<SyncIssueErrorCountsMessage>
    {
        private readonly IResetIssueErrorCountsCommand _resetIssueErrorCountsCommand;

        public SyncIssueCountsConsumer(IResetIssueErrorCountsCommand resetIssueErrorCountsCommand)
        {
            _resetIssueErrorCountsCommand = resetIssueErrorCountsCommand;
        }

        public void Consume(SyncIssueErrorCountsMessage message)
        {
            _resetIssueErrorCountsCommand.Invoke(new ResetIssueErrorCountsRequest
            {
                IssueId = message.IssueId,
                TriggerEventUtc = message.TriggerEventUtc,
            });
        }
    }
}
