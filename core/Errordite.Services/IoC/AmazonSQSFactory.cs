using Amazon;
using Amazon.SQS;
using Errordite.Services.Configuration;

namespace Errordite.Services.IoC
{
    public interface IAmazonSQSFactory
    {
        AmazonSQS Create();
    }

    public class AmazonSQSFactory : IAmazonSQSFactory
    {
        private readonly ServiceConfigurationContainer _serviceConfigurationContainer;

        public AmazonSQSFactory(ServiceConfigurationContainer serviceConfigurationContainer)
        {
            _serviceConfigurationContainer = serviceConfigurationContainer;
        }

        public AmazonSQS Create()
        {
            return AWSClientFactory.CreateAmazonSQSClient(
                _serviceConfigurationContainer.Configuration.AWSAccessKey,
                _serviceConfigurationContainer.Configuration.AWSSecretKey,
                RegionEndpoint.EUWest1);
        }
    }
}
