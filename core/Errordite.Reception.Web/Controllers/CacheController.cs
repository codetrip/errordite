using System.Web.Mvc;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.IoC;
using System.Linq;

namespace Errordite.Reception.Web.Controllers
{
    public class CacheController : Controller
    {
        public ActionResult Clear()
        {
            foreach (var cacheEngine in ObjectFactory.Container.ResolveAll<ICacheEngine>().Where(e => !e.Name.ToLowerInvariant().Contains("hybrid")))
                cacheEngine.Clear();

            return Content("Cache Flushed");
        }
    }
}
