using System.Web;
using System.Web.Mvc;

namespace Errordite.Samples.Mvc3.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult LogIn(string username)
        {
            Response.SetCookie(new HttpCookie("login", username));
            return Redirect(HttpContext.Request.UrlReferrer.ToString());
        }

        public ActionResult LogOut()
        {
            Response.SetCookie(new HttpCookie("login", null));
            return Redirect(HttpContext.Request.UrlReferrer.ToString());
        }
    }
}