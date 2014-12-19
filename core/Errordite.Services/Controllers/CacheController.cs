using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Errordite.Core.Caching;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Invalidation;
using Errordite.Core.Domain.Error;
using Errordite.Core.Issues;
using Errordite.Core.Web;

namespace Errordite.Services.Controllers
{
    public class CacheController : ErrorditeApiController
    {
        private readonly ICacheConfiguration _cacheConfiguration;
        private readonly ICacheEngine _cacheEngine;
	    private readonly IReceptionServiceIssueCache _issueCache;

        public CacheController(ICacheEngine cacheEngine, 
			ICacheConfiguration cacheConfiguration,
			IReceptionServiceIssueCache issueCache)
        {
            _cacheEngine = cacheEngine;
            _cacheConfiguration = cacheConfiguration;
	        _issueCache = issueCache;
        }

		public IEnumerable<IssueBase> Get(string orgId, string appId)
		{
			return _issueCache.GetIssues(appId, orgId);
		}

        public HttpResponseMessage Delete(string orgId)
        {
            var cacheInvalidator = new CacheInvalidator(_cacheConfiguration)
	        {
		        Auditor = Auditor
	        };

	        cacheInvalidator.SetCacheEngine(_cacheEngine);
            cacheInvalidator.Invalidate(new CacheInvalidationItem(
                CacheProfiles.Organisations,
                CacheKeys.Organisations.Key(orgId)));

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage Delete(string orgId, string applicationId)
        {
			var cacheInvalidator = new CacheInvalidator(_cacheConfiguration)
			{
				Auditor = Auditor
			};

            cacheInvalidator.SetCacheEngine(_cacheEngine);
            cacheInvalidator.Invalidate(new CacheInvalidationItem(
                CacheProfiles.Applications,
                CacheKeys.Applications.Key(orgId, applicationId)));

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}