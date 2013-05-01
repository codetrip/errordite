using Errordite.Test.Automation.Data;
using NUnit.Framework;
using Errordite.Core.Extensions;

namespace Errordite.Test.Automation.Tests
{
    [TestFixture]
    public class OrganisationTests : AutomatedTestBase
    {
        [Test]
        public void RegisterOrganisation()
        {
            Armoury.ErrorditeDriver.Register();
            Assert.That(LoggedInUser != null);
            Armoury.ErrorditeDriver.Login();
        }

        [Test]
        public void CreateAndDeleteApplication()
        {
            Armoury.ErrorditeDriver.Register();
            string applicationId = Armoury.ErrorditeDriver.AddApplication(TestConstants.TestApplication.Name).Id;
            Armoury.ErrorditeDriver.DeleteApplication(applicationId);
            Armoury.ErrorditeDriver.Logout();
        }

        [Test]
        public void CreateAndDeleteGroup()
        {
            Armoury.ErrorditeDriver.Register();
            var groupId = Armoury.ErrorditeDriver.CreateGroup(TestConstants.TestGroup.Name);
            Assert.That(groupId.IsNotNullOrEmpty());
            Armoury.ErrorditeDriver.DeleteGroup(groupId);
            Armoury.ErrorditeDriver.Logout();
        }
    }
}