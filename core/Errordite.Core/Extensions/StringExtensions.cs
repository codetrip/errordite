using System.Web;

namespace Errordite.Core.Extensions
{
    public static class StringExtensions
    {
        public static string MergeQueryStrings(this string query1, string query2)
        {
            var nvc1 = HttpUtility.ParseQueryString(query1);
            var nvc2 = HttpUtility.ParseQueryString(query2);
            return nvc1.MergeWith(nvc2).ToQueryString();
        }
    }
}
