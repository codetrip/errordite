using System;
using System.Collections.Generic;
using System.Linq;
using CodeTrip.Core;
using CodeTrip.Core.Caching;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues.Queries;

namespace Errordite.Core.Issues
{
    public interface IReceptionServiceIssueCache
    {
        IEnumerable<IssueBase> GetIssues(string applicationId);
        void Add(IssueBase issue);
        void Update(IssueBase issue);
        void Delete(string issueId, string applicationId);
    }

    public class ReceptionServiceIssueCache : ComponentBase, IReceptionServiceIssueCache
    {
        private static readonly ObjectCache<List<IssueBase>> _cache = new ObjectCache<List<IssueBase>>();
        private readonly static object _syncLock = new object();
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetAllApplicationIssuesQuery _getIssues;

        public ReceptionServiceIssueCache(IGetAllApplicationIssuesQuery getIssues, 
            ErrorditeConfiguration configuration)
        {
            _getIssues = getIssues;
            _configuration = configuration;
        }

        public IEnumerable<IssueBase> GetIssues(string applicationId)
        {
            lock(_syncLock)
            {
                var issues = GetCachedIssues(Application.GetId(applicationId));
                return issues.OrderByDescending(i => i.MatchPriority).ThenByDescending(i => i.LastErrorUtc);
            }
        }

        public void Add(IssueBase issue)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(issue.ApplicationId);

                if (issues.FindIndex(m => m.Id == issue.Id) == -1)
                    issues.Add(issue);
            }
        }

        public void Update(IssueBase issue)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(issue.ApplicationId);

                var index = issues.FindIndex(m => m.Id == issue.Id);

                if (index >= 0)
                    issues[index] = issue;
            }
        }

        public void Delete(string issueId, string applicationId)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(Application.GetId(applicationId));

                var index = issues.FindIndex(m => m.Id == Issue.GetId(issueId));

                if (index >= 0)
                    issues.RemoveAt(index);
            }
        }

        private List<IssueBase> GetCachedIssues(string applicationId)
        {
            var issues = _cache.Get(applicationId);

            if (issues == null)
            {
                Trace("Attempting to load issues for application:={0}", applicationId);

                var request = new GetAllApplicationIssuesRequest
                {
                    ApplicationId = applicationId
                };

                issues = _getIssues.Invoke(request).Issues ?? new List<IssueBase>();

                _cache.Add(applicationId, issues, DateTimeOffset.UtcNow.AddMinutes(_configuration.IssueCacheTimeoutMinutes));

                Trace("Successfully loaded {0} issues", issues.Count);
            }
            
            return issues;
        }
    }
}
