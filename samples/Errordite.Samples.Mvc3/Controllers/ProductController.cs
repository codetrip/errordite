using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Errordite.Samples.Mvc3.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Index(string id)
        {
            var product = DataHelper.Get(id);
            var loginCookie = Request.Cookies["login"];

            return View(new ProductPageViewModel
                {
                    Basket = BasketHelper.Get(),
                    Product = product,
                    LoginName = loginCookie != null && loginCookie.Value != "" ? loginCookie.Value: null,
                    ErrorMessage = TempData["error-message"] as string,
                    MoreLikeThis = DataHelper.MoreLike(id),
                });
        }

    }

    public class UserNotLoggedInOnAddingToBasketException : Exception
    {
    }

    public abstract class ControllerBase : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            
        }   
    }

    

    public class ProductPageViewModel
    {
        public Product Product { get; set; }
        public List<Product> Basket { get; set; }

        public string LoginName { get; set; }
        public string ErrorMessage { get; set; }

        public IEnumerable<Product> MoreLikeThis { get; set; }
    }

    public class Product
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        public string Name { get; set; }
    }
}