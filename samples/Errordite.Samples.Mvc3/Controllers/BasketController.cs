using System.Web.Mvc;

namespace Errordite.Samples.Mvc3.Controllers
{
    [HandleError(View = "AcmeError")]
    public class BasketController : ControllerBase
    {
        public ActionResult Add(string productId)
        {
            var loginCookie = Request.Cookies["login"];
            if (loginCookie == null || string.IsNullOrEmpty(loginCookie.Value))
                throw new UserNotLoggedInOnAddingToBasketException();

            var product = DataHelper.Get(productId);

            if (!BasketHelper.Add(product))
                TempData.Add("error-message", "Sorry we could not add this item to your basket.");

            return RedirectToAction("index", "product", new { id = productId });
        }        
    }
}