using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using System.Linq;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.Authentication.Commands
{
    public class AuthenticateUserCommand : SessionAccessBase, IAuthenticateUserCommand
    {
        private readonly IGetOrganisationsByEmailAddressCommand _getOrganisationsByEmailAddressCommand;

        public AuthenticateUserCommand(IGetOrganisationsByEmailAddressCommand getOrganisationsByEmailAddressCommand)
        {
            _getOrganisationsByEmailAddressCommand = getOrganisationsByEmailAddressCommand;
        }

        public AuthenticateUserResponse Invoke(AuthenticateUserRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");
            ArgumentValidation.NotEmpty(request.Password, "request.Password");

            var response = _getOrganisationsByEmailAddressCommand.Invoke(new GetOrganisationsByEmailAddressRequest
            {
                EmailAddress = request.Email,
            });

			if (response.UserMapping == null || response.UserMapping.Password != request.Password.Hash() || response.Organisations == null)
			{
				return new AuthenticateUserResponse
				{
					Status = AuthenticateUserStatus.LoginFailed
				};
			}

	        var organisations = response.Organisations.ToList();

			if (organisations.Count > 1 && request.OrganisationId.IsNullOrEmpty())
			{
				return new AuthenticateUserResponse
				{
					Status = AuthenticateUserStatus.MultipleOrganisations
				};
			}

	        var organisation = request.OrganisationId.IsNullOrEmpty() ? 
				organisations.First() : 
				organisations.FirstOrDefault(o => o.Id == Organisation.GetId(request.OrganisationId)) ?? organisations.First();

            Session.SetOrganisation(organisation);

            Trace("Getting user {0} from org {1} with pwdhash {2}", request.Email, organisation.Id, request.Password.Hash());

            var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email.ToLowerInvariant());

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
		public string OrganisationId { get; set; }
    }

    public enum AuthenticateUserStatus
    {
        Ok,
        AccountInactive,
        OrganisationInactive,
        LoginFailed,
		MultipleOrganisations
    }
}
