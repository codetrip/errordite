
using System;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using Mindscape.Raygun4Net;

namespace Errordite.Samples.Mvc4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                throw new Exception("test exception");
            }
            catch (Exception ex)
            {
                new RaygunClient().Send(ex);
                throw;
            }
        }

        public ActionResult ErrorInView()
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
