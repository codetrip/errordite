using System;
using System.Diagnostics;
using Amazon;
using Amazon.SQS;
using Amazon.SimpleEmail;
using NUnit.Framework;

namespace Errordite.Test.Messaging
{
    [TestFixture]
    public class MessagingTests : ErrorditeTestBase
    {
        [Test]
        public void PerfAmazonClients()
        {
            const int iterations = 1;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
               var email =  new AmazonSimpleEmailServiceClient("AKIAIZV7WJHA3YGIBV7A", "lOk5snPLHdV+I/AMC6CQfUKG2kMu4IuYRgKYJI+c", new AmazonSimpleEmailServiceConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1
                });
            }

            Console.WriteLine(watch.ElapsedMilliseconds);

            for (int i = 0; i < iterations; i++)
            {
                var sqs = new AmazonSQSClient("AKIAIZV7WJHA3YGIBV7A", "lOk5snPLHdV+I/AMC6CQfUKG2kMu4IuYRgKYJI+c", new AmazonSQSConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1
                });
            }

            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}