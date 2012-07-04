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
                var issues = GetCachedIssues(applicationId);
                return issues.OrderByDescending(i => i.MatchPriority).ThenByDescending(i => i.LastErrorUtc);
            }
        }

        public void Add(IssueBase issue)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(issue.ApplicationId);

                var index = issues.FindIndex(m => m.Id == issue.Id);

                if (index == -1)
                {
                    Trace("Adding new issue to cache with Id:-{0}", issue.Id);
                    issues.Add(issue);
                }
                else
                {
                    Error("Updating issue in cache (from add method!) at index {0} with Id:={1}", index, issue.Id);
                    issues[index] = issue;
                } 
            }
        }

        public void Update(IssueBase issue)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(issue.ApplicationId);

                var index = issues.FindIndex(m => m.Id == issue.Id);

                if (index >= 0)
                {
                    Trace("Updating issue in cache at index {0} with Id:={1}", index, issue.Id);
                    issues[index] = issue;
                }
                else
                {
                    Error("Adding new issue to cache (from update method!) with Id:-{0}", issue.Id);
                    issues.Add(issue);
                }
            }
        }

        public void Delete(string issueId, string applicationId)
        {
            lock (_syncLock)
            {
                var issues = GetCachedIssues(applicationId);

                var index = issues.FindIndex(m => m.Id == Issue.GetId(issueId));

                if (index >= 0)
                {
                    issues.RemoveAt(index);
                }
                else
                {
                    Error("Failed to locate issue with Id {0} in the issue cache for applicationId:={1}", issueId, applicationId);
                }
            }
        }

        private List<IssueBase> GetCachedIssues(string applicationId)
        {
            applicationId = Application.GetId(applicationId);

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
