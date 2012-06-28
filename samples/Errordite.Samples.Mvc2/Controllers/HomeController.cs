
using System.Web.Mvc;

namespace Errordite.Samples.Mvc2.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Error()
        {
            int zero = 0;
            int test = 100/zero;
            return View();
        }
    }
}
