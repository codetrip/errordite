namespace Errordite.Core.Domain
{
    public static class IdHelper
    {
        public static string GetFriendlyId(string idFriendlyOrNormal)
        {
            var parts = idFriendlyOrNormal.Split('/');
            return parts.Length == 2 ? parts[1] : idFriendlyOrNormal;
        }
    }
}