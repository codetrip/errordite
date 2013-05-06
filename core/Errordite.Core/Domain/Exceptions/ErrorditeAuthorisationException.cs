
using Errordite.Core.Exceptions;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Domain.Exceptions
{
    public class ErrorditeAuthorisationException : ErrorditeException
    {
        public ErrorditeAuthorisationException(IOrganisationEntity entity, User user)
        {
            Data.Add("UserId", user.Id);
            Data.Add("OrganisationId", user.OrganisationId);
            Data.Add("OrganisationEntity-Name", entity.GetType().Name);
            Data.Add("OrganisationEntity-OrganisationId", entity.OrganisationId);
        }

        public ErrorditeAuthorisationException(IUserEntity entity, User user)
        {
            Data.Add("User-Id", user.Id);
            Data.Add("User-OrganisationId", user.OrganisationId);
            Data.Add("UserEntity-Name", entity.GetType().Name);
            Data.Add("UserEntity-UserId", entity.UserId);
        }
    }
}
