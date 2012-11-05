
using Errordite.Core.Indexing;

namespace Errordite.Core
{
    public static class CoreConstants
    {
        public const string MatchRuleFactoryIdFormat = "mf-{0}";

        public static class FacetDocuments
        {
            public static string IssueStatus = "facets/Status";
        }

        public static class IndexNames
        {
            public static string ErrorditeErrors = new ErrorditeErrors().IndexName;
            public static string Audit = new AuditRecords().IndexName;
            public static string Errors = new Errors_Search().IndexName;
            public static string Issues = new Issues_Search().IndexName;
            public static string UnloggedErrors = new UnloggedErrors().IndexName;
            public static string UserAlerts = new UserAlerts_Search().IndexName;
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
