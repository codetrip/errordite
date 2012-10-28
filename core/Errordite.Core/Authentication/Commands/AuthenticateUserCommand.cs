using CodeTrip.Core;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;
using Errordite.Core.Indexing;
using System.Linq;
using Errordite.Core.Organisations.Commands;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Authentication.Commands
{
    public class AuthenticateUserCommand : SessionAccessBase, IAuthenticateUserCommand
    {
        private IGetOrganisationByEmailAddressCommand _getOrganisationByEmailAddressCommand;

        public AuthenticateUserCommand(IGetOrganisationByEmailAddressCommand getOrganisationByEmailAddressCommand)
        {
            _getOrganisationByEmailAddressCommand = getOrganisationByEmailAddressCommand;
        }

        public AuthenticateUserResponse Invoke(AuthenticateUserRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");
            ArgumentValidation.NotEmpty(request.Password, "request.Password");

            var org = _getOrganisationByEmailAddressCommand.Invoke(new GetOrganisationByEmailAddressRequest
                {
                    EmailAddress = request.Email,
                }).Organisation;

            if (org == null)
                return new AuthenticateUserResponse{Status = AuthenticateUserStatus.LoginFailed};

            var user = Session.Raven.Query<User, Users_Search>()
                .FirstOrDefault(u => u.Email == request.Email.ToLowerInvariant() && u.Password == request.Password.Hash());

            if (user != null)
            {
                if(!user.Status.Equals(UserStatus.Active))
                {
                    return new AuthenticateUserResponse
                    {
                        Status = AuthenticateUserStatus.AccountInactive
                    };
                }

                var organisation = Session.MasterRaven.Query<Organisation, Organisations_Search>().FirstOrDefault(o => o.Id == Organisation.GetId(user.OrganisationId));

                if (organisation == null || organisation.Status == OrganisationStatus.Suspended)
                {
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
