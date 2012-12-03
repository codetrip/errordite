using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Web;
using Errordite.Core.Configuration;
using Errordite.Core.Identity;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Notifications;
using CodeTrip.Core.Extensions;

namespace Errordite.Web.Extensions
{
    public static class ViewDataExtensions
    {
        #region AppContext & Configuration

        private const string BreadcrumbKey = "breadcrumb_key";
        private const string AppContextKey = "app_context_key";
        private const string ConfigurationKey = "configuration_key";
        private const string CookieManagerKey = "cookiemanager_key";

        public static ErrorditeConfiguration GetConfiguration(this ViewDataDictionary viewData)
        {
            return viewData[ConfigurationKey] as ErrorditeConfiguration;
        }

        public static void SetErrorditeConfiguration(this ViewDataDictionary viewData, ErrorditeConfiguration configuration)
        {
            viewData[ConfigurationKey] = configuration;
        }

        public static List<Breadcrumb> GetBreadcrumbs(this ViewDataDictionary viewData)
        {
            return viewData[BreadcrumbKey] as List<Breadcrumb>;
        }

        public static void SetBreadcrumbs(this ViewDataDictionary viewData, List<Breadcrumb> breadcrumbs)
        {
            viewData[BreadcrumbKey] = breadcrumbs;
        }

        public static AppContext GetAppContext(this ViewDataDictionary viewData)
        {
            return viewData[AppContextKey] as AppContext ?? AppContext.Null;
        }

        public static void SetAppContext(this ViewDataDictionary viewData, AppContext appContext)
        {
            viewData[AppContextKey] = appContext;
        }

        public static string GetIssuesUrl(this ViewDataDictionary viewData, string baseUrl)
        {
            return GetQuery(viewData, baseUrl, WebConstants.CookieSettings.IssueSearchCookieKey);
        }

        public static string GetErrorsUrl(this ViewDataDictionary viewData, string baseUrl)
        {
            return GetQuery(viewData, baseUrl, WebConstants.CookieSettings.ErrorSearchCookieKey);
        }

        private static string GetQuery(this ViewDataDictionary viewData, string baseUrl, string cookieName)
        {
            var cookieManager = viewData[CookieManagerKey] as ICookieManager;

            if (cookieManager == null)
                return baseUrl;

            string query = cookieManager.Get(cookieName);
            return query.IsNullOrEmpty() ? baseUrl : "{0}{1}".FormatWith(baseUrl, query);
        }

        public static void SetCoookieManager(this ViewDataDictionary viewData, ICookieManager cookieManager)
        {
            viewData[CookieManagerKey] = cookieManager;
        }

        #endregion

        #region Notifications

        private const string NotificationKey = "notifications";

        public static void SetNotification(this ViewDataDictionary viewData, UiNotification uiNotification)
        {
            viewData[NotificationKey] = uiNotification;
        }

        public static UiNotification GetNotification(this ViewDataDictionary viewData)
        {
            return viewData[NotificationKey] == null ? null : (UiNotification)viewData[NotificationKey];
        }

        public static bool HasNotification(this ViewDataDictionary viewData)
        {
            return viewData[NotificationKey] != null;
        }

        #endregion
    }
}