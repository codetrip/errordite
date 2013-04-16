using System.Web.Mvc;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.IoC;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.Models.Cache;
using Errordite.Web.Extensions;
using CodeTrip.Core.Extensions;
using System.Linq;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class CacheController : ErrorditeController
    {
        private readonly ICacheConfiguration _cacheConfiguration;

        public CacheController(ICacheConfiguration cacheConfiguration)
        {
            _cacheConfiguration = cacheConfiguration;
        }

        [ImportViewData, GenerateBreadcrumbs(BreadcrumbId.AdminCache)]
        public ActionResult Index(CacheProfiles id, string engine)
        {
            engine = engine.IsNullOrEmpty() ? ObjectFactory.GetObject<ICacheEngine>().Name : engine;
            var cacheEngine = ObjectFactory.GetObject<ICacheEngine>(engine);

            return View(new CacheViewModel
            {
                Cache = id,
                CacheEngine = cacheEngine.Name.ToLowerInvariant(),
                Keys = cacheEngine.Keys(_cacheConfiguration.GetCacheProfile(id)),
                Engines = ObjectFactory.Container.ResolveAll<ICacheEngine>().ToSelectList(e => e.Name, e => e.Name, e => e.Name.ToLowerInvariant() == engine.ToLowerInvariant())
            });
        }

        [ExportViewData]
        public ActionResult FlushAllCaches()
        {
            foreach (var cacheEngine in ObjectFactory.Container.ResolveAll<ICacheEngine>().Where(e => !e.Name.ToLowerInvariant().Contains("hybrid")))
                cacheEngine.Clear();

            ConfirmationNotification("Successfully flushed all caches");
            return Redirect(Request.UrlReferrer == null ? Url.SystemAdmin() : Request.UrlReferrer.AbsoluteUri);
        }

        [ExportViewData]
        public ActionResult Delete(CacheProfiles cache, string cacheKey, string engine)
        {
            engine = engine.IsNullOrEmpty() ? ObjectFactory.GetObject<ICacheEngine>().Name : engine;
            var cacheEngine = ObjectFactory.GetObject<ICacheEngine>(engine);
            var key = CacheKey
                .ForProfile(_cacheConfiguration.GetCacheProfile(cache))
                .WithKey(cacheKey)
                .Create();

            cacheEngine.Remove(key);

            ConfirmationNotification("Item with key {0} was successfully removed from the cache".FormatWith(key.Key));
            return RedirectToAction("index", new { id = cache.ToString().ToLowerInvariant(), engine });
        }

        [ExportViewData]
        public ActionResult Flush(CacheProfiles cache, string engine)
        {
            engine = engine.IsNullOrEmpty() ? ObjectFactory.GetObject<ICacheEngine>().Name : engine;
            var cacheEngine = ObjectFactory.GetObject<ICacheEngine>(engine);
            cacheEngine.Clear(_cacheConfiguration.GetCacheProfile(cache));
            ConfirmationNotification("Profile {0} was successfully flushed".FormatWith(cache.ToString()));
            return RedirectToAction("index", new { id = cache.ToString().ToLowerInvariant(), engine });
        }
    }
}
