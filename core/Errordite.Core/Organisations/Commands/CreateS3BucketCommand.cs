using Amazon.S3;
using Amazon.S3.Model;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
    public class CreateS3BucketCommand : SessionAccessBase, ICreateS3BucketCommand
    {
        private readonly AmazonS3 _amazonS3;
        private readonly ErrorditeConfiguration _configuration;

        public CreateS3BucketCommand(ErrorditeConfiguration configuration, AmazonS3 amazonS3)
        {
            _configuration = configuration;
            _amazonS3 = amazonS3;
        }

        public CreateS3BucketResponse Invoke(CreateS3BucketRequest request)
        {
            Trace("Starting...");
            Trace("...Attempting to create bucket:=errordite-buckets-{0}".FormatWith(request.OrganisationId.GetFriendlyId()));

            _amazonS3.PutBucket(new PutBucketRequest
            {
                BucketName = "errordite-buckets-{0}".FormatWith(request.OrganisationId.GetFriendlyId()),
                BucketRegion = S3Region.EU
            });

            return new CreateS3BucketResponse
            {
                Status = CreateS3BucketStatus.Ok
            };
        }
    }

    public interface ICreateS3BucketCommand : ICommand<CreateS3BucketRequest, CreateS3BucketResponse>
    { }

    public class CreateS3BucketResponse
    {
        public CreateS3BucketStatus Status { get; set; }
    }

    public class CreateS3BucketRequest
    {
        public string OrganisationId { get; set; }
    }

    public enum CreateS3BucketStatus
    {
        Ok
    }
}
