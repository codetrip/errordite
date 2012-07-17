
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Reception.Commands;
using Errordite.Core.WebApi;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Linq;

namespace Errordite.Test.IntegrationTests
{
    [TestFixture]
    public class ReceptionIssueCacheTests
    {
        [Test]
        public void SendErrorsForProcessing()
        {
            var errors = GetErrors();

            var postTask = new HttpClient().PostJsonAsync("{0}/api/ReprocessIssueErrors".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint), errors);

            postTask.Wait();
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.OK);

            var read = postTask.Result.Content.ReadAsStringAsync();
            read.Wait();
            var responses = JsonConvert.DeserializeObject<IEnumerable<ReceiveErrorResponse>>(read.Result);
            Assert.That(responses.Count() == 2);
        }

        [Test]
        public void PutNewIssueInCache()
        {
            var issue = GetIssue();

            var postTask = new HttpClient().PostJsonAsync("{0}/api/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint), issue);

            postTask.Wait();
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            issue.LastErrorUtc = DateTime.UtcNow;

            //now update
            var putTask = new HttpClient().PutJsonAsync("{0}/api/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint), new []{issue});
            putTask.Wait();
            Assert.That(putTask.Result.StatusCode == HttpStatusCode.OK);

            var task = new HttpClient().GetAsync(
                "{0}/api/issue/{1}?applicationId={2}".FormatWith(
                    Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint, issue.FriendlyId, issue.ApplicationId));

            task.Wait();

            var readResponseTask = task.Result.Content.ReadAsStringAsync();
            readResponseTask.Wait();
            var retrievedIssue = JsonConvert.DeserializeObject<Issue>(readResponseTask.Result);
            Assert.That(retrievedIssue.LastErrorUtc == issue.LastErrorUtc);
            Assert.That(task.Result.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void PostNewIssueInCache()
        {
            var issue = GetIssue();

            var postTask = new HttpClient().PostJsonAsync("{0}/api/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint), issue);

            postTask.Wait();
            Console.Write(postTask.Result.StatusCode);
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            var task =
                new HttpClient().GetAsync(
                    "{0}/api/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint, issue.FriendlyId, issue.ApplicationId));

            task.Wait();
            Console.Write(task.Result.StatusCode);
            Assert.That(task.Result.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void PostNewIssueInCacheAndDelete()
        {
            var issue = GetIssue();

            var postTask = new HttpClient().PostAsJsonAsync("{0}/api/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint), issue);

            postTask.Wait();
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            var task =
                new HttpClient().GetAsync(
                    "{0}/api/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint, issue.FriendlyId, issue.ApplicationId));

            task.Wait();
            Assert.That(task.Result.StatusCode == HttpStatusCode.OK);

            string id = "{0}|{1}".FormatWith(issue.FriendlyId, IdHelper.GetFriendlyId(issue.ApplicationId));

            var deleteTask = new HttpClient().DeleteAsync("{0}/api/issue/{1}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint, id));

            deleteTask.Wait();
            Assert.That(deleteTask.Result.StatusCode == HttpStatusCode.NoContent);

            var getTask =
                new HttpClient().GetAsync(
                    "{0}/api/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceptionHttpEndpoint, issue.Id, issue.ApplicationId));

            getTask.Wait();
            Assert.That(getTask.Result.StatusCode == HttpStatusCode.NotFound);
        }

        private Issue GetIssue()
        {
            return new Issue
            {
                ApplicationId = "applications/97",
                Id = "issues/1",
                Name = "Test Issue",
                CreatedOnUtc = DateTime.UtcNow,
                ErrorCount = 10,
                LastErrorUtc = DateTime.UtcNow.AddMinutes(-10),
                UserId = "users/12",
                LimitStatus = ErrorLimitStatus.Ok,
                MatchPriority = MatchPriority.Low,
                OrganisationId = "organisations/1"
            };
        }

        private IEnumerable<Error> GetErrors()
        {
            return new[]
            {
                new Error
                {
                    MachineName = "MachineName1",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = "application/97",
                    OrganisationId = "organisations/1",
                    ExceptionInfo = new ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }
                },
                new Error
                {
                    MachineName = "MachineName1",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = "application/97",
                    OrganisationId = "organisations/1",
                    ExceptionInfo = new ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }
                }
            };
        }
    }
}
