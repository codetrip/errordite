using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Session;

namespace Errordite.Core.Messaging.Commands
{
    public class CreateSQSQueueCommand : SessionAccessBase, ICreateSQSQueueCommand
    {
        private readonly AmazonSQS _amazonSQS;
        private readonly ErrorditeConfiguration _configuration;

        public CreateSQSQueueCommand(AmazonSQS amazonSqs, ErrorditeConfiguration configuration)
        {
            _amazonSQS = amazonSqs;
            _configuration = configuration;
        }

        public CreateSQSCommandResponse Invoke(CreateSQSCommandRequest request)
        {
            Trace("Starting...");

            _amazonSQS.CreateQueue(new CreateQueueRequest
            {
                DefaultVisibilityTimeout = _configuration.QueueVisibilityTimeoutSeconds,
                QueueName = "errordite-receive-{0}".FormatWith(request.OrganisationId.GetFriendlyId())
            });

            return new CreateSQSCommandResponse
            {
                Status = CreateSQSCommandStatus.Ok
            };
        }
    }

    public interface ICreateSQSQueueCommand : ICommand<CreateSQSCommandRequest, CreateSQSCommandResponse>
    { }

    public class CreateSQSCommandResponse
    {
        public CreateSQSCommandStatus Status { get; set; }
    }

    public class CreateSQSCommandRequest
    {
        public string OrganisationId { get; set; }
    }

    public enum CreateSQSCommandStatus
    {
        Ok
    }
}
