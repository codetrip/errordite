using System.Collections.Generic;
using Errordite.Core.Caching.Entities;
using System.Linq;

namespace Errordite.Core.Caching
{
    public static class CacheInvalidation
    {
        public static IEnumerable<CacheInvalidationItem> GetGroupInvalidationItems(string organisationId, string groupId = null)
        {
            if (groupId != null)
                yield return new CacheInvalidationItem(CacheProfiles.Groups, CacheKeys.Groups.Key(organisationId, groupId));

            yield return new CacheInvalidationItem(CacheProfiles.Groups, CacheKeys.Groups.PerOrganisationPrefix(organisationId), true);
            yield return new CacheInvalidationItem(CacheProfiles.Users, CacheKeys.Users.PerOrganisationPrefix(organisationId), true);
            yield return new CacheInvalidationItem(CacheProfiles.Applications, CacheKeys.Applications.PerOrganisationPrefix(organisationId), true);
        }

        public static IEnumerable<CacheInvalidationItem> GetApplicationInvalidationItems(string organisationId, string applicationId = null)
        {
            if (applicationId != null)
                yield return new CacheInvalidationItem(CacheProfiles.Applications, CacheKeys.Applications.Key(organisationId, applicationId));

            yield return new CacheInvalidationItem(CacheProfiles.Applications, CacheKeys.Applications.PerOrganisationPrefix(organisationId), true);
        }

        public static IEnumerable<CacheInvalidationItem> GetUserInvalidationItems(string organisationId, string userId = null, string email = null)
        {
            if (userId != null)
                yield return new CacheInvalidationItem(CacheProfiles.Users, CacheKeys.Users.Key(organisationId, userId));

            if (email != null)
                yield return new CacheInvalidationItem(CacheProfiles.Users, CacheKeys.Users.Email(email));

            yield return new CacheInvalidationItem(CacheProfiles.Users, CacheKeys.Users.PerOrganisationPrefix(organisationId), true);
        }

        public static IEnumerable<CacheInvalidationItem> GetOrganisationInvalidationItems(string organisationId, string email = null)
        {
            if (email != null)
            {
                yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Email(email));
            }

            yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Key());
            yield return new CacheInvalidationItem(CacheProfiles.Organisations, CacheKeys.Organisations.Key(organisationId));
            foreach (var item in GetUserInvalidationItems(organisationId)
                .Union(GetApplicationInvalidationItems(organisationId))
                .Union(GetGroupInvalidationItems(organisationId)))
                yield return item;
        }
    }
}
