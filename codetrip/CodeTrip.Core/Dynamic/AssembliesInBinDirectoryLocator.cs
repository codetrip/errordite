using System;
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Web.Hosting;

namespace CodeTrip.Core.Dynamic
{
    public class AssembliesInBinDirectoryLocator
    {
        /// <summary>
        /// Returns locations of all assemblies in the bin directory of the application.
        /// </summary>
        /// <param name="result">The assembly locations.</param>
        /// <returns>True.</returns>
        public bool Locate(out string[] result)
        {
            result = Directory.GetFiles(GetBinFolder(), "*.dll");
            return true;
        }

        /// <summary>
        /// Returns the bin folder for the current application type
        /// </summary>
        /// <returns></returns>
        private static string GetBinFolder()
        {
            string location;

            if (HttpContext.Current != null)
            {
                // web (IIS/WCF ASP compatibility mode)context
                location = HttpRuntime.BinDirectory;
            }
            else if (OperationContext.Current != null)
            {
                // pure wcf context
                location = HostingEnvironment.ApplicationPhysicalPath;
            }
            else
            {
                // no special hosting context (console/winform etc)
                location = AppDomain.CurrentDomain.BaseDirectory;
            }

            return location;
        }
    }
}
