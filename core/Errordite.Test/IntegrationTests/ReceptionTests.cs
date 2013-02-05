using System;
using Errordite.Core.Applications.Commands;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Matching;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Reception.Commands;
using Errordite.Core.Session;
using NUnit.Framework;
using System.Linq;

namespace Errordite.Test.IntegrationTests
{
    [TestFixture]
    public class ReceptionTests : ErrorditeTestBase
    {
        [Test]
        public void ReceiveTwoIdenticalErrors()
        {
            var ravenSession = Get<IAppSession>().Raven;
            var org = ravenSession.Query<Organisation, Organisations_Search>().FirstOrDefault(o => o.Name == "UnitTests");
            if (org != null)
                ravenSession.Delete(org); //TODO: delete child entities

            var user = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.Email == "unittests@codetrip.co.uk");
            if (user != null)
                ravenSession.Delete(user);

            ravenSession.SaveChanges();

            var orgInfo = Get<ICreateOrganisationCommand>().Invoke(
                new CreateOrganisationRequest()
                    {
                        Email = "unittests@codetrip.co.uk",
                        FirstName = "Unit",
                        LastName = "Tests",
                        OrganisationName = "UnitTests",
                        Password = "password"
                    });

            var matchRuleFactory = Get<IMatchRuleFactoryFactory>().Create().First();

            var appInfo = Get<IAddApplicationCommand>().Invoke(new AddApplicationRequest
            {
                IsActive = true,
                MatchRuleFactoryId = matchRuleFactory.Id,
                Name = "UnitTestApp1",
                CurrentUser = new User { OrganisationId = orgInfo.OrganisationId },
                UserId = orgInfo.UserId,
            });

            var app = ravenSession.Query<Application, Applications_Search>().First(a => a.OrganisationId == orgInfo.OrganisationId && a.Name == "UnitTestApp1");

            Get<IReceiveErrorCommand>().Invoke(new ReceiveErrorRequest
            {
                Error = new Error
                {
                    MachineName = "MachineName1",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = app.Id,
                    OrganisationId = app.OrganisationId,
                    ExceptionInfos = new[] {new Core.Domain.Error.ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }}.ToArray()
                }
            });

            Get<IReceiveErrorCommand>().Invoke(new ReceiveErrorRequest
            {
                Error = new Error
                {
                    MachineName = "MachineName1",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = app.Id,
                    ExceptionInfos = new [] {new Core.Domain.Error.ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }}.ToArray()
                }
            });

            var errorInstances = ravenSession.Query<Error>().Where(
                e => e.ApplicationId == app.Id && e.ExceptionInfos.First().Type == "ExceptionType1").ToArray();

            ravenSession.SaveChanges();

            Assert.That(errorInstances.Length, Is.EqualTo(2));
            Assert.That(errorInstances[0].IssueId, Is.EqualTo(errorInstances[1].IssueId));
        }
    }
}