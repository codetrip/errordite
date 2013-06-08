using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Web;
using Errordite.Core;
using Errordite.Core.Configuration;
using Errordite.Core.Identity;
using Errordite.Web.Models.Dashboard;
using Errordite.Web.Models.Navigation;
using Errordite.Web.Models.Notifications;
using Errordite.Core.Extensions;

namespace Errordite.Web.Extensions
{
    public static class ViewDataExtensions
    {
        #region AppContext & Configuration

        private const string BreadcrumbKey = "breadcrumb_key";
        private const string AppContextKey = "app_context_key";
        private const string ConfigurationKey = "configuration_key";
        private const string CookieManagerKey = "cookiemanager_key";
		private const string ErrorditeCoreKey = "errordite_core_key";
		private const string ActiveTabKey = "active_tab_key";

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

        public static string GetSelectedApplication(this ViewDataDictionary viewData, string baseUrl)
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

        public static string GetSelectedApplication(this ViewDataDictionary viewData)
        {
            var cookieManager = viewData[CookieManagerKey] as ICookieManager;

            if (cookieManager == null)
                return null;

            return cookieManager.Get(WebConstants.CookieSettings.ApplicationIdCookieKey);
        }

        public static void SetCore(this ViewDataDictionary viewData, IErrorditeCore errorditeCore)
        {
            viewData[ErrorditeCoreKey] = errorditeCore;
        }

        public static IErrorditeCore GetCore(this ViewDataDictionary viewData)
        {
            return viewData[ErrorditeCoreKey] as IErrorditeCore;
		}

		public static void SetActiveTab(this ViewDataDictionary viewData, NavTabs activeTab)
		{
			viewData[ActiveTabKey] = activeTab;
		}

		public static NavTabs GetActiveTab(this ViewDataDictionary viewData)
		{
			return (NavTabs)viewData[ActiveTabKey];
		}

        public static void SetCookieManager(this ViewDataDictionary viewData, ICookieManager cookieManager)
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