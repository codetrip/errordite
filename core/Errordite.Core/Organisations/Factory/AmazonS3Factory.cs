using Amazon;
using Amazon.S3;
using Errordite.Core.Configuration;

namespace Errordite.Core.Organisations.Factory
{
    public interface IAmazonS3Factory
    {
        AmazonS3Client Create();
    }

    public class AmazonS3Factory : IAmazonS3Factory
    {
        private readonly ErrorditeConfiguration _configuration;

        public AmazonS3Factory(ErrorditeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AmazonS3Client Create()
        {
            return new AmazonS3Client(_configuration.AWSAccessKey, _configuration.AWSSecretKey, new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.EUWest1
            });
        }
    }
}
