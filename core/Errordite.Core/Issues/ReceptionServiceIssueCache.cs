using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Caching;
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
        private static readonly Dictionary<string, ObjectCache<ListHolder<IssueBase>>> _cache = new Dictionary<string, ObjectCache<ListHolder<IssueBase>>>();
        private readonly object _cacheSyncLock = new object();
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
            return GetCachedIssues(applicationId, organisationId).List;
        }

        public void Add(IssueBase issue)
        {
            var appIssues = GetCachedIssues(issue.ApplicationId, issue.OrganisationId);

            lock (appIssues.SyncLock)
            {
                var index = appIssues.List.FindIndex(m => m.Id == issue.Id);

                if (index == -1)
                {
                    Trace("Adding new issue to cache with Id:-{0}", issue.Id);
                    appIssues.List.Add(issue);
                    //TODO: would be more efficient to go through the list looking for the right spot to add the new issue
                    
                }
                else
                {
                    Error("Updating issue in cache (from add method!) at index {0} with Id:={1}", index, issue.Id);
                    appIssues.List[index] = issue;
                }

                OrderIssues(appIssues);
            }
        }

        public void Update(IssueBase issue)
        {
            var appIssues = GetCachedIssues(issue.ApplicationId, issue.OrganisationId);

            lock (appIssues.SyncLock)
            {
                //TODO: would be better to have a List<Ref<IssueBase>> and update it directly perhaps
                var index = appIssues.List.FindIndex(m => m.Id == issue.Id);

                if (index >= 0)
                {
                    Trace("Updating issue in cache at index {0} with Id:={1}", index, issue.Id);
                    appIssues.List[index] = issue;
                }
                else
                {
                    Error("Adding new issue to cache (from update method!) with Id:-{0}", issue.Id);
                    appIssues.List.Add(issue);
                }
                
                OrderIssues(appIssues);
            }
        }

        private static void OrderIssues(ListHolder<IssueBase> appIssues)
        {
            //PLEASE DO NOT CHANGE THIS WITHOUT DISCUSSIONss
            appIssues.List = appIssues.List
                .OrderByDescending(i => i.LastRuleAdjustmentUtc ?? DateTime.MinValue)
                .ToList();
        }

        public void Delete(string issueId, string applicationId, string organisationId)
        {
            var appIssues = GetCachedIssues(applicationId, organisationId);

            lock (appIssues.SyncLock)
            {
                var index = appIssues.List.FindIndex(m => m.Id == Issue.GetId(issueId));

                if (index >= 0)
                {
                    appIssues.List.RemoveAt(index);
                    //no need to sort at this point as we won't have changed the order
                }
                else
                {
                    Error("Failed to locate issue with Id {0} in the issue cache for applicationId:={1}", issueId, applicationId);
                }
            }
        }

        private class ListHolder<T>
        {
			public List<T> List { get; set; }
            public object SyncLock { get { return _syncLock; } }
            private readonly object _syncLock = new object();
        }

        private ListHolder<IssueBase> GetCachedIssues(string applicationId, string organisationId)
        {
            applicationId = Application.GetId(applicationId);
            organisationId = Organisation.GetId(organisationId);

            ObjectCache<ListHolder<IssueBase>> orgCache;
            if (!_cache.TryGetValue(organisationId, out orgCache))
            {
                lock (_cacheSyncLock)
                {
                    if (!_cache.TryGetValue(organisationId, out orgCache))
                    {
                        orgCache = new ObjectCache<ListHolder<IssueBase>>();
                        _cache.Add(organisationId, orgCache);
                    }
                }
            }

            var issues = orgCache.Get(applicationId);

            if (issues == null)
            {
                Trace("Attempting to load issues for application:={0}", applicationId);

                lock (_cacheSyncLock)
                {
                    issues = new ListHolder<IssueBase>()
                    {
                        List = _getIssues.Invoke(new GetAllApplicationIssuesRequest
                        {
                            ApplicationId = applicationId
                        }).Issues ?? new List<IssueBase>()
                    };

                    orgCache.Add(applicationId, issues, DateTimeOffset.UtcNow.AddMinutes(_configuration.IssueCacheTimeoutMinutes));

                    Trace("Successfully loaded {0} issues", issues.List.Count);
                }
            }
            
            return issues;
        }
    }
}
