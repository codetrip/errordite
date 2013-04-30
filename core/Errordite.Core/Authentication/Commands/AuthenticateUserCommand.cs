using Errordite.Core;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using System.Linq;
using Errordite.Core.Organisations.Commands;
using Errordite.Core.Organisations.Queries;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Authentication.Commands
{
    public class AuthenticateUserCommand : SessionAccessBase, IAuthenticateUserCommand
    {
        private readonly IGetOrganisationByEmailAddressCommand _getOrganisationByEmailAddressCommand;

        public AuthenticateUserCommand(IGetOrganisationByEmailAddressCommand getOrganisationByEmailAddressCommand)
        {
            _getOrganisationByEmailAddressCommand = getOrganisationByEmailAddressCommand;
        }

        public AuthenticateUserResponse Invoke(AuthenticateUserRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");
            ArgumentValidation.NotEmpty(request.Password, "request.Password");

            var organisation = _getOrganisationByEmailAddressCommand.Invoke(new GetOrganisationByEmailAddressRequest
                {
                    EmailAddress = request.Email,
                }).Organisation;

            if (organisation == null)
                return new AuthenticateUserResponse{Status = AuthenticateUserStatus.LoginFailed};

            Session.SetOrganisation(organisation);

            Trace("Getting user {0} from org {1} with pwdhash {2}", request.Email, organisation.Id, request.Password.Hash());
            var user = Session.Raven.Query<User, Indexing.Users>()
                .FirstOrDefault(u => u.Email == request.Email.ToLowerInvariant() && u.Password == request.Password.Hash());

            if (user != null)
            {
                if(!user.Status.Equals(UserStatus.Active))
                {
                    Trace("account inactive");
                    return new AuthenticateUserResponse
                    {
                        Status = AuthenticateUserStatus.AccountInactive
                    };
                }

                if (organisation.Status == OrganisationStatus.Suspended)
                {
                    Trace("org inactive");
                    return new AuthenticateUserResponse
                    {
                        Status = AuthenticateUserStatus.OrganisationInactive
                    };
                }

                return new AuthenticateUserResponse
                {
                    UserId = user.Id,
                    OrganisationId = user.OrganisationId,
                    Status = AuthenticateUserStatus.Ok
                };
            }

            return new AuthenticateUserResponse
            {
                Status = AuthenticateUserStatus.LoginFailed
            };
        }
    }

    public interface IAuthenticateUserCommand : ICommand<AuthenticateUserRequest, AuthenticateUserResponse>
    { }

    public class AuthenticateUserResponse
    {
        public string UserId { get; set; }
        public string OrganisationId { get; set; }
        public AuthenticateUserStatus Status { get; set; }
    }

    public class AuthenticateUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public enum AuthenticateUserStatus
    {
        Ok,
        AccountInactive,
        OrganisationInactive,
        LoginFailed
    }
}
