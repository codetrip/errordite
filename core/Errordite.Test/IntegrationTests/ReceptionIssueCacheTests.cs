
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Errordite.Core.Extensions;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues.Commands;
using Errordite.Core.Matching;
using Errordite.Core.Web;
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
            var postTask = new HttpClient().PostJsonAsync("{0}/1/ReprocessIssueErrors".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint), new ReprocessIssueErrorsRequest
            {
                IssueId = "issues/1",
                OrganisationId = "organisations/1"
            });

            postTask.Wait();
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.OK);

            var read = postTask.Result.Content.ReadAsStringAsync();
            read.Wait();
            var response = JsonConvert.DeserializeObject<ReprocessIssueErrorsResponse>(read.Result);
            Assert.That(response.Status == ReprocessIssueErrorsStatus.Ok);
        }

        [Test]
        public void PutNewIssueInCache()
        {
            var issue = GetIssue();

            var postTask = new HttpClient().PostJsonAsync("{0}/1/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint), issue);

            postTask.Wait();

            if (postTask.Result.StatusCode != HttpStatusCode.Created)
            {
                var read = postTask.Result.Content.ReadAsStringAsync();
                read.Wait();
                Console.Write(read.Result);
            }

            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            issue.LastErrorUtc = DateTime.UtcNow;

            //now update
            var putTask = new HttpClient().PutJsonAsync("{0}/1/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint), new[] { issue });
            putTask.Wait();

            Assert.That(putTask.Result.StatusCode == HttpStatusCode.NoContent);

            var task = new HttpClient().GetAsync(
                "{0}/1/issue/{1}?applicationId={2}".FormatWith(
                    Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint, issue.FriendlyId, issue.ApplicationId));

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

            var postTask = new HttpClient().PostJsonAsync("{0}/1/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint), issue);

            postTask.Wait();
            Console.Write(postTask.Result.StatusCode);
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            var task =
                new HttpClient().GetAsync(
                    "{0}/1/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint, issue.FriendlyId, issue.ApplicationId));

            task.Wait();
            Console.Write(task.Result.StatusCode);
            Assert.That(task.Result.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void PostNewIssueInCacheAndDelete()
        {
            var issue = GetIssue();

            var postTask = new HttpClient().PostAsJsonAsync("{0}/1/issue".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint), issue);

            postTask.Wait();
            Assert.That(postTask.Result.StatusCode == HttpStatusCode.Created);

            var task =
                new HttpClient().GetAsync(
                    "{0}/1/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint, issue.FriendlyId, issue.ApplicationId));

            task.Wait();
            Assert.That(task.Result.StatusCode == HttpStatusCode.OK);

            string id = "{0}|{1}".FormatWith(issue.FriendlyId, IdHelper.GetFriendlyId(issue.ApplicationId));

            var deleteTask = new HttpClient().DeleteAsync("{0}/1/issue/{1}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint, id));

            deleteTask.Wait();
            Assert.That(deleteTask.Result.StatusCode == HttpStatusCode.NoContent);

            var getTask =
                new HttpClient().GetAsync(
                    "{0}/1/issue/{1}?applicationId={2}".FormatWith(Core.Configuration.ErrorditeConfiguration.Current.ReceiveServiceEndpoint, issue.Id, issue.ApplicationId));

            getTask.Wait();
            Assert.That(getTask.Result.StatusCode == HttpStatusCode.NotFound);
        }

        private Issue GetIssue()
        {
            return new Issue
            {
                ApplicationId = "applications/1",
                Id = "issues/1",
                Name = "Test Issue",
                CreatedOnUtc = DateTime.UtcNow,
                ErrorCount = 10,
                LastErrorUtc = DateTime.UtcNow.AddMinutes(-10),
                UserId = "users/1",
                LimitStatus = ErrorLimitStatus.Ok,
                OrganisationId = "organisations/1",
                Rules = new List<IMatchRule> {new PropertyMatchRule("Module", StringOperator.Equals, "Source1")}
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
                    ApplicationId = "application/1",
                    OrganisationId = "organisations/1",
                    ExceptionInfos = new[] { new ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }}.ToArray()
                },
                new Error
                {
                    MachineName = "MachineName1",
                    TimestampUtc = DateTime.UtcNow,
                    ApplicationId = "application/1",
                    OrganisationId = "organisations/1",
                    ExceptionInfos = new[] {new ExceptionInfo
                    {

                        StackTrace = "Description1",
                        Type = "ExceptionType1",
                        Message = "Message1",
                        Module = "Source1"
                    }}.ToArray()
                }
            };
        }
    }
}
