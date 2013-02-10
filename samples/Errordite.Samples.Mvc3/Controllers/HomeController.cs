
using System;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Web.Mvc;
using log4net;

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

        public ActionResult NeedsParam(int param)
        {
            return Content(param.ToString());
        }

        public ActionResult ErrorInView()
        {
            return View();
        }

        public ActionResult Error(int? index, string errorMessage)
        {
	        var logger = LogManager.GetLogger("Errordite.Samples");
			logger.Debug(string.Format("Logging error index:={0}, message:={1}", index, errorMessage));

            switch (index)
            {
                case 1:
                    {
						logger.Debug("Case 1");
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
						logger.Debug("Case 2");
                        if (index == 2)
                            throw new ArgumentException(errorMessage + "The argument cannot be 2", "index");
                    }
                    break;
                case 3:
					{
						logger.Debug("Case 3");
                        throw new ArgumentNullException("index", errorMessage + "parameter index cannot be null");
                    }
                case 4:
					{
						logger.Debug("Case 4");
                        throw new ConfigurationErrorsException(errorMessage + "Some config was invalid, cannot continue");
                    }
                case 5:
					{
						logger.Debug("Case 5");
                        throw new IOException("with an inner exception", new EventLogException("inner message"));
                    }
                default:
					{
						logger.Debug("Case Default");
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
