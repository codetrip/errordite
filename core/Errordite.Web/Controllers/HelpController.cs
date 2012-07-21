using System.Web.Mvc;
using Errordite.Web.ActionFilters;
using Errordite.Web.Models.Navigation;

namespace Errordite.Web.Controllers
{
    public class HelpController : ErrorditeController
    {
        [GenerateBreadcrumbs(BreadcrumbId.Faq)]
        public ActionResult Faq()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Pricing)]
        public ActionResult Pricing()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Help)]
        public ActionResult GettingStarted()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Features)]
        public ActionResult Features()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Client)]
        public ActionResult Client()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Client)]
        public ActionResult SendErrorWithJson()
        {
            return View();
        }

        [GenerateBreadcrumbs(BreadcrumbId.Privacy)]
        public ActionResult Privacy()
        {
            return View();
        }

        //[GenerateBreadcrumbs(BreadcrumbId.TermsAndConditions)]
        //public ActionResult TermsAndConditions()
        //{
        //    return View();
        //}
    }
}
