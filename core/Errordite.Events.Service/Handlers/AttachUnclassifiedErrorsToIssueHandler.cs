using Errordite.Core.Errors.Commands;
using Errordite.Core.Errors.Queries;
using Errordite.Core.Messages;
using Errordite.Core.ServiceBus;

namespace Errordite.Events.Service.Handlers
{
    public class AttachUnclassifiedErrorsToIssueHandler : MessageHandlerSessionBase<AttachUnclassifiedErrorsToIssueMessage>
    {
        private readonly IGetUnclassifiedErrorsThatMatchRulesQuery _getUnclassifiedErrorsThatMatchRulesQuery;
        private readonly IMoveErrorsToNewIssueCommand _moveErrorsToNewIssueCommand;

        public AttachUnclassifiedErrorsToIssueHandler(IGetUnclassifiedErrorsThatMatchRulesQuery getUnclassifiedErrorsThatMatchRulesQuery, 
            IMoveErrorsToNewIssueCommand moveErrorsToNewIssueCommand)
        {
            _getUnclassifiedErrorsThatMatchRulesQuery = getUnclassifiedErrorsThatMatchRulesQuery;
            _moveErrorsToNewIssueCommand = moveErrorsToNewIssueCommand;
        }

        protected override void HandleMessage(AttachUnclassifiedErrorsToIssueMessage message)
        {
            var response = _getUnclassifiedErrorsThatMatchRulesQuery.Invoke(new GetUnclassifiedErrorsThatMatchRulesRequest
            {
                IssueId = message.IssueId
            });

            if (response.Errors != null && response.Errors.Count > 0)
            {
                _moveErrorsToNewIssueCommand.Invoke(new MoveErrorsToNewIssueRequest
                {
                    Errors = response.Errors,
                    IssueId = message.IssueId
                });
            }
        }
    }
}
