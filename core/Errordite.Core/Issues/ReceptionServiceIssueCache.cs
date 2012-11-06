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
        IEnumerable<IssueBase> GetIssues(string applicationId, string organisationId);
        void Add(IssueBase issue);
        void Update(IssueBase issue);
        void Delete(string issueId, string applicationId, string organisationId);
    }

    public class ReceptionServiceIssueCache : ComponentBase, IReceptionServiceIssueCache
    {
        private static readonly Dictionary<string, ObjectCache<List<IssueBase>>> _cache = new Dictionary<string, ObjectCache<List<IssueBase>>>();
        private readonly object _syncLock = new object();
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetAllApplicationIssuesQuery _getIssues;

        public ReceptionServiceIssueCache(IGetAllApplicationIssuesQuery getIssues, 
            ErrorditeConfiguration configuration)
        {
            _getIssues = getIssues;
            _configuration = configuration;
        }

        public IEnumerable<IssueBase> GetIssues(string applicationId, string organisationId)
        {
            return GetCachedIssues(applicationId, organisationId);
        }

        public void Add(IssueBase issue)
        {
            var issues = GetCachedIssues(issue.ApplicationId, issue.OrganisationId);

            lock (issues)
            {
                var index = issues.FindIndex(m => m.Id == issue.Id);

                if (index == -1)
                {
                    Trace("Adding new issue to cache with Id:-{0}", issue.Id);
                    issues.Add(issue);
                    issues = issues.OrderByDescending(i => i.MatchPriority).ThenByDescending(i => i.LastErrorUtc).ToList();
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
            var issues = GetCachedIssues(issue.ApplicationId, issue.OrganisationId);

            lock (issues)
            {
                //TODO: would be better to have a List<Ref<IssueBase>> and update it directly perhaps
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
                    issues = issues.OrderByDescending(i => i.MatchPriority).ThenByDescending(i => i.LastErrorUtc).ToList();
                }
            }
        }

        public void Delete(string issueId, string applicationId, string organisationId)
        {
            var issues = GetCachedIssues(applicationId, organisationId);

            lock (issues)
            {
                var index = issues.FindIndex(m => m.Id == Issue.GetId(issueId));

                if (index >= 0)
                {
                    issues.RemoveAt(index);
                    issues = issues.OrderByDescending(i => i.MatchPriority).ThenByDescending(i => i.LastErrorUtc).ToList();
                }
                else
                {
                    Error("Failed to locate issue with Id {0} in the issue cache for applicationId:={1}", issueId, applicationId);
                }
            }
        }

        private List<IssueBase> GetCachedIssues(string applicationId, string organisationId)
        {
            applicationId = Application.GetId(applicationId);
            organisationId = Organisation.GetId(organisationId);

            ObjectCache<List<IssueBase>> orgCache;
            if (!_cache.TryGetValue(organisationId, out orgCache))
            {
                lock (_syncLock)
                {
                    if (!_cache.TryGetValue(organisationId, out orgCache))
                    {
                        orgCache = new ObjectCache<List<IssueBase>>();
                        _cache.Add(organisationId, orgCache);
                    }
                }
            }

            var issues = orgCache.Get(applicationId);

            if (issues == null)
            {
                Trace("Attempting to load issues for application:={0}", applicationId);

                lock (orgCache)
                {
                    issues = _getIssues.Invoke(new GetAllApplicationIssuesRequest
                    {
                        ApplicationId = applicationId
                    }).Issues ?? new List<IssueBase>();

                    orgCache.Add(applicationId, issues, DateTimeOffset.UtcNow.AddMinutes(_configuration.IssueCacheTimeoutMinutes));

                    Trace("Successfully loaded {0} issues", issues.Count);
                }
            }
            
            return issues;
        }
    }
}
