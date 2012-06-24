
using System;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Web.Mvc;

namespace Errordite.Samples.Mvc3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Error(int? index, string errorMessage)
        {
            switch (index)
            {
                case 1:
                    {
                        try
                        {
                            int zero = 0;
                            int test = 100 / zero;
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException(errorMessage + string.Format("Cannot divide by zero! hresult:={0}", new Random().Next(10000)), e);
                        }
                    }
                    break;
                case 2:
                    {
                        if (index == 2)
                            throw new ArgumentException(errorMessage + "The argument cannot be 2", "index");
                    }
                    break;
                case 3:
                    {
                        throw new ArgumentNullException("index", errorMessage + "parameter index cannot be null");
                    }
                case 4:
                    {
                        throw new ConfigurationErrorsException(errorMessage + "Some config was invalid, cannot continue");
                    }
                case 5:
                    {
                        throw new IOException("with an inner exception", new EventLogException("inner message"));
                    }
                default:
                    {
                        throw new Exception(errorMessage);
                    }
            }
            
            return RedirectToAction("index");
        }

        public ActionResult Product()
        {
            return View();
        }
    }
}
