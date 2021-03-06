﻿
namespace Errordite.Web
{
    public static class WebConstants
    {
        public static class AreaNames
        {
            public const string System = "system";
        }

        public static class CookieSettings
        {
            public const string IssueSearchCookieKey = "isck";
            public const string ErrorSearchCookieKey = "esck";
			public const string ApplicationIdCookieKey = "appid";
			public const string DashboardCookieKey = "dashboard";
        }

        public static class RouteValues
        {
			public const string SetApplication = "setapp";
			public const string SetOrganisation = "setorg";
        }
    }
}