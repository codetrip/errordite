using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Caching
{
    public static class CacheKeys
    {
        public static class Organisations
        {
            public static string Key()
            {
                return "organisations";
            }

            public static string Key(string organisationId)
            {
                return Organisation.GetId(organisationId);
            }

            public static string Statistics(string organisationId)
            {
                return "o-{0}-stats".FormatWith(Organisation.GetId(organisationId));
            }
        }

        public static class Groups
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "g-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Key(string organisationId, string groupId)
            {
                return "{0}-{1}".FormatWith(PerOrganisationPrefix(organisationId), Group.GetId(groupId));
            }
        }

        public static class Applications
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "a-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Key(string organisationId, string applicationId)
            {
                return "{0}-{1}".FormatWith(PerOrganisationPrefix(organisationId), Application.GetId(applicationId));
            }
        }

        public static class Users
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "u-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Key(string organisationId, string userId)
            {
                return "{0}-{1}".FormatWith(PerOrganisationPrefix(organisationId), User.GetId(userId));
            }

            public static string Email(string email)
            {
                return "{0}".FormatWith(email);
            }
        }
    }
}