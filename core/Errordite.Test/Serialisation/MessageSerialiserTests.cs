using Errordite.Core.Domain.Error;
using Errordite.Core.Messaging;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Errordite.Test.Serialisation
{
    [TestFixture]
    public class MessageSerialiserTests : ErrorditeTestBase
    {
        [Test]
        public void RavenTest()
        {
            var msg = new ReceiveErrorMessage
            {
                ApplicationId = "wqewq",
                OrganisationId = "sdsfds",
                ExistingIssueId = "",
                Token = "trdgfd",
                Error = new Error()
            };

            var msgText = JsonConvert.SerializeObject(msg);

            var msgBase = JsonConvert.DeserializeObject<MessageBase>(msgText);
            Assert.That(msgBase != null);
        }
    }
}