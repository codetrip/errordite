using System.Web.Mvc;
using Errordite.Core.Caching;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Caching.Invalidation;

namespace Errordite.Receive.Controllers
{
    public class CacheController : Controller
    {
        private readonly ICacheConfiguration _cacheConfiguration;
        private readonly ICacheEngine _cacheEngine;

        public CacheController(ICacheConfiguration cacheConfiguration, ICacheEngine cacheEngine)
        {
            _cacheConfiguration = cacheConfiguration;
            _cacheEngine = cacheEngine;
        }

        public ActionResult Clear()
        {
            _cacheEngine.Clear();
            return Content("Cache Flushed");
        }

        [HttpDelete]
        public ActionResult Flush(string organisationId)
        {
            var cacheInvalidator = new CacheInvalidator(_cacheConfiguration);

            cacheInvalidator.SetCacheEngine(_cacheEngine);
            cacheInvalidator.Invalidate(new CacheInvalidationItem(
                CacheProfiles.Organisations, 
                CacheKeys.Organisations.Key(organisationId)));

            return Content("Cache Flushed");
        }

        [HttpDelete]
        public ActionResult Flush(string organisationId, string applicationId)
        {
            var cacheInvalidator = new CacheInvalidator(_cacheConfiguration);

            cacheInvalidator.SetCacheEngine(_cacheEngine);
            cacheInvalidator.Invalidate(new CacheInvalidationItem(
                CacheProfiles.Applications,
                CacheKeys.Applications.Key(organisationId, applicationId)));

            return Content("Cache Flushed");
        }
    }
}
