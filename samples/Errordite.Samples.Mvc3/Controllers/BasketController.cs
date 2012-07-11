using System.Web.Mvc;

namespace Errordite.Samples.Mvc3.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddToBasket(int productId)
        {
            return Content("added");
        }
    }
}