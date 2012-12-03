using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;

namespace CodeTrip.Core.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the build number from the Shared Assembly Info
        /// </summary>
        /// <param name="callingAssembly">Current executing assembly</param>
        /// <returns>Build number</returns>
        public static string GetCurrentBuildNumber(this Assembly callingAssembly)
        {
            string buildNumber = string.Empty;

            if (callingAssembly != null && !String.IsNullOrEmpty(callingAssembly.Location))
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(callingAssembly.Location);
                buildNumber = HttpUtility.UrlEncode(fileVersionInfo.ProductVersion);
            }

            return buildNumber;
        }
    }
}
