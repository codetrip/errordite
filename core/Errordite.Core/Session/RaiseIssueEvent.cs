using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using CodeTrip.Core.Extensions;
using System.Linq;
using Errordite.Core.WebApi;
using Newtonsoft.Json;

namespace Errordite.Core.Session
{
    public class RaiseIssueCreatedEvent : SessionCommitAction
    {
        private readonly Issue _issue;

        public RaiseIssueCreatedEvent(Issue issue)
            : base("RaiseIssueCreatedEvent")
        {
            _issue = issue;
        }

        public override void Execute(IAppSession session)
        {
            session.ReceptionServiceHttpClient.PostJsonAsync("issue", _issue.ToIssueBase());
        }
    }

    public class RaiseIssueModifiedEvent : SessionCommitAction
    {
        private readonly IEnumerable<Issue> _issues;

        public RaiseIssueModifiedEvent(Issue issue)
            : this(new []{issue})
        {}

        public RaiseIssueModifiedEvent(IEnumerable<Issue> issues)
            : base("RaiseIssueModifiedEvent")
        {
            _issues = issues;
        }

        public override void Execute(IAppSession session)
        {
            var issues = _issues.Select(i => i.ToIssueBase());
            session.ReceptionServiceHttpClient.PutJsonAsync("issue", issues);
        }
    }

    /// <summary>
    /// Need to send a delimited set of Issue ids with their applicationId encoded in the string, i.e. IssueId|ApplicationId^IssueId|ApplicationId
    /// This class ensures the iDs are correctly formatted before sending them to the reception service http endpoint
    /// </summary>
    public class RaiseIssueDeletedEvent : SessionCommitAction
    {
        private readonly string _issueIds;

        public RaiseIssueDeletedEvent(string issueIds) :
            base("RaiseIssueDeletedEvent")
        {
            _issueIds = issueIds;
        }

        public override void Execute(IAppSession session)
        {
            var ids = new StringBuilder();

            //ensure the ids are in the correct format
            foreach(string id in _issueIds.Split(new []{'^'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var idparts = id.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                ids.Append("{0}|{1}^".FormatWith(IdHelper.GetFriendlyId(idparts[0]), IdHelper.GetFriendlyId(idparts[1])));
            }

            session.ReceptionServiceHttpClient.DeleteAsync("issue/{0}".FormatWith(ids.ToString().TrimEnd(new []{'^'})));
        }
    }
}
