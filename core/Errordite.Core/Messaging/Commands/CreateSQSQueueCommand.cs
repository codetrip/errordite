using System;
using Amazon.SQS;
using Amazon.SQS.Model;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Session;
using System.Linq;

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
            Trace("...Attempting to create queue:={0}", request.QueueUrl);

            var response = _amazonSQS.CreateQueue(new CreateQueueRequest
            {
                DefaultVisibilityTimeout = _configuration.QueueVisibilityTimeoutSeconds,
                QueueName = new Uri(request.QueueUrl).Segments.Last(),
            });

            Trace("Completed, queue '{0}' created", response.CreateQueueResult.QueueUrl);

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
        public string QueueUrl { get; set; }
    }

    public enum CreateSQSCommandStatus
    {
        Ok
    }
}
