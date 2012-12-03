
using Errordite.Core.Domain.Exceptions;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Authorisation
{
    public interface IAuthorisationManager
    {
        void Authorise(IOrganisationEntity entity, User currentUser);
        void Authorise(IUserEntity entity, User currentUser);
    }

    public class AuthorisationManager : IAuthorisationManager
    {
        public void Authorise(IOrganisationEntity entity, User currentUser)
        {
            if (ReferenceEquals(currentUser, User.System()))
                return;

            if (!entity.OrganisationId.ToLowerInvariant().Equals(currentUser.OrganisationId.ToLowerInvariant()))
                throw new ErrorditeAuthorisationException(entity, currentUser);
        }

        public void Authorise(IUserEntity entity, User currentUser)
        {
            if (ReferenceEquals(currentUser, User.System()))
                return;

            if (!entity.UserId.ToLowerInvariant().Equals(currentUser.Id.ToLowerInvariant()))
                throw new ErrorditeAuthorisationException(entity, currentUser);
        }
    }

    public interface IOrganisationEntity
    {
        string OrganisationId { get; }
    }

    public interface IUserEntity
    {
        string UserId { get; }
    }
}
