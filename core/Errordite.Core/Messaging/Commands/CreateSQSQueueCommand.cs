using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Configuration;
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

        public CreateSQSQueueResponse Invoke(CreateSQSQueueRequest request)
        {
            Trace("Starting...");
			Trace("...Attempting to create queue:={0}", request.QueueName);

            var response = _amazonSQS.CreateQueue(new CreateQueueRequest
            {
                DefaultVisibilityTimeout = _configuration.QueueVisibilityTimeoutSeconds,
				QueueName = request.QueueName,
            });

            Trace("Completed, queue '{0}' created", response.CreateQueueResult.QueueUrl);

            return new CreateSQSQueueResponse
            {
                Status = CreateSQSCommandStatus.Ok
            };
        }
    }

    public interface ICreateSQSQueueCommand : ICommand<CreateSQSQueueRequest, CreateSQSQueueResponse>
    { }

    public class CreateSQSQueueResponse
    {
        public CreateSQSCommandStatus Status { get; set; }
    }

    public class CreateSQSQueueRequest
    {
        public string QueueName { get; set; }
    }

    public enum CreateSQSCommandStatus
    {
        Ok
    }
}
