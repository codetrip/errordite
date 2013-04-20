using Amazon;
using Amazon.SimpleEmail;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.Sending
{
    public interface IAmazonSimpleEmailFactory
    {
        AmazonSimpleEmailService Create();
    }

    public class AmazonSimpleEmailFactory : IAmazonSimpleEmailFactory
    {
        private readonly ErrorditeConfiguration _configuration;

        public AmazonSimpleEmailFactory(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AmazonSimpleEmailService Create()
        {
            return AWSClientFactory.CreateAmazonSimpleEmailServiceClient(
                _configuration.AWSAccessKey,
                _configuration.AWSSecretKey,
                RegionEndpoint.EUWest1);
        }
    }
}
