using System.Web.Mvc;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.IoC;
using Errordite.Web.ActionFilters;
using Errordite.Web.Controllers;
using Errordite.Web.Extensions;

namespace Errordite.Web.Areas.System.Controllers
{
    [Authorize, RoleAuthorize]
    public class CacheController : ErrorditeController
    {
        [ExportViewData]
        public ActionResult FlushAllCaches()
        {
            var engine = ObjectFactory.GetObject<ICacheEngine>();
            engine.Clear();

            ConfirmationNotification("Successfully flushed all caches");
            return Redirect(Request.UrlReferrer == null ? Url.SystemAdmin() : Request.UrlReferrer.AbsoluteUri);
        }
    }
}
