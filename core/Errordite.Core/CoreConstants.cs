
using Errordite.Core.Indexing;

namespace Errordite.Core
{
    public static class CoreConstants
    {
        public const string ErrorditeMasterDatabaseName = "Errordite";
        public const string MatchRuleFactoryIdFormat = "mf-{0}";
		public const string QueryDateFormat = "yyyy-MM-ddTHH:mm:ss.fff";
		public const string OrganisationIdCookieKey = "orgid";

        public static class Auditing
        {
            public const string DefaultLogger = "Errordite";
        }

        public static class FacetDocuments
        {
            public static string IssueStatus = "facets/Status";
        }

        public static class IndexNames
        {
            public static string Errors = new Indexing.Errors().IndexName;
            public static string Issues = new Indexing.Issues().IndexName;
            public static string IssueDailyCount = new IssueDailyCounts().IndexName;
            public static string OrganisationIssueDailyCount = new OrganisationDailyCounts().IndexName;
            public static string UserOrganisationMappings = new UserOrganisationMappings().IndexName;
			public static string Organisations = new Indexing.Organisations().IndexName;
			public static string IssueHistory = new History().IndexName;
        }

        public static class Authentication
        {
            public const string IdentityCookieName = "m-aid";
            public const string UserId = "uid";
            public const string OrganisationId = "oid";
            public const string Email = "em";
            public const string RememberMe = "rm";
            public const string HasUserProfile = "hp";
            public const string GuestUserName = "Guest User";
            public const string IsAuthenticated = "au";
        }

        public static class ExceptionKeys
        {
            public const string User = "User";
            public const string Url = "Url";
            public const string UserAgent = "User-Agent";
            public const string Form = "Form";
            public const string Password = "Password";
            public const string UploadedFiles = "Uploaded-Files";
        }

        public static class SortFields
        {
            public const string TimestampUtc = "TimestampUtc";
            public const string LastErrorUtc = "LastErrorUtc";
            public const string CompletedOnUtc = "CompletedOnUtc";
        }
    }
}
