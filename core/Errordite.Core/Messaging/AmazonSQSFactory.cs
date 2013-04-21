using Amazon;
using Errordite.Core.Configuration;

namespace Errordite.Core.Messaging
{
    public interface IAmazonSQSFactory
    {
        Amazon.SQS.AmazonSQS Create();
    }

    public class AmazonSQSFactory : IAmazonSQSFactory
    {
        private readonly ErrorditeConfiguration _configuration;

        public AmazonSQSFactory(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Amazon.SQS.AmazonSQS Create()
        {
            return AWSClientFactory.CreateAmazonSQSClient(_configuration.AWSAccessKey, _configuration.AWSSecretKey, RegionEndpoint.EUWest1);
        }
    }
}
