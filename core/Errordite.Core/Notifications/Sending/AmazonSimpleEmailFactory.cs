using Amazon;
using Amazon.SimpleEmail;
using Errordite.Core.Configuration;

namespace Errordite.Core.Notifications.Sending
{
    public interface IAmazonSimpleEmailFactory
    {
        AmazonSimpleEmailServiceClient Create();
    }

    public class AmazonSimpleEmailFactory : IAmazonSimpleEmailFactory
    {
        private readonly ErrorditeConfiguration _configuration;

        public AmazonSimpleEmailFactory(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AmazonSimpleEmailServiceClient Create()
        {
            return new AmazonSimpleEmailServiceClient(_configuration.AWSAccessKey, _configuration.AWSSecretKey, new AmazonSimpleEmailServiceConfig
            {
                RegionEndpoint = RegionEndpoint.EUWest1
            });
        }
    }
}
