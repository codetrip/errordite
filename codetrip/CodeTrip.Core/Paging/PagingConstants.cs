namespace CodeTrip.Core.Paging
{
    public static class PagingConstants
    {
        public const string DefaultPagingId = "default";

        public static class QueryStringParameters
        {
            public const string PagingPrefix = "pg";
            public const string PageNumber = PagingPrefix + "no";
            public const string PageSize = PagingPrefix + "sz";
            public const string PageSort = PagingPrefix + "st";
            public const string PageSortDescending = PagingPrefix + "sd";
            public const string PageTab = "tab";
        }
    }
}