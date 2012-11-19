
using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.Models.Issues
{
    public class IssueActionViewModel
    {
        public string PagingMessage { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowPrevious { get; set; }
        public IEnumerable<IssueItemViewModel> Issues { get; set; }
    }

    public class IssueItemViewModel
    {
        public string FormKey { get; set; }
        public string IssueId { get; set; }
        public string ApplicationId { get; set; }
        public DateTime LastErrorUtc { get; set; }
        public int ErrorCount { get; set; }
        public string UserName { get; set; }
        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Priority { get; set; }
        public bool Selected { get; set; }
        public IssueStatus Status { get; set; }

        public static List<IssueItemViewModel> FromIssues(IEnumerable<Issue> issues, IEnumerable<Application> applications, IEnumerable<User> users)
        {
            return issues.Select(issue => new IssueItemViewModel
            {
                IssueId = issue.FriendlyId,
                ErrorCount = issue.ErrorCount,
                LastErrorUtc = issue.LastErrorUtc,
                Name = issue.Name,
                Status = issue.Status,
                UserName = users.FirstOrDefault(u => u.Id == issue.UserId).IfPoss(u => u.FullName, issue.UserId),
                ApplicationName = applications.FirstOrDefault(a => a.Id == issue.ApplicationId).IfPoss(a => a.Name, issue.ApplicationId),
                Selected = true,
                Priority = Resources.IssueResources.ResourceManager.GetString("IssuePriority_{0}".FormatWith(issue.MatchPriority.ToString())),
                FormKey = "issue_{0}".FormatWith(issue.FriendlyId),
                ApplicationId = issue.ApplicationId
            }).ToList();
        }
    }
}