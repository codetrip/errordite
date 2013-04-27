using System;
using System.Diagnostics;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
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

        [Test]
        public void SendMessageToNonExistentQueue()
        {
            try
            {
                var sqs = new AmazonSQSClient("AKIAIZV7WJHA3YGIBV7A", "lOk5snPLHdV+I/AMC6CQfUKG2kMu4IuYRgKYJI+c", new AmazonSQSConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1
                });

                sqs.ReceiveMessage(new ReceiveMessageRequest
                {
                    QueueUrl = "https://sqs.eu-west-1.amazonaws.com/186350237634/errordite-receive-11",
                    MaxNumberOfMessages = 1
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().AssemblyQualifiedName);
            }
            
        }
    }
}