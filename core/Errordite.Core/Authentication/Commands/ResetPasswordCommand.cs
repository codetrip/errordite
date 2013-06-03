using System;
using Errordite.Core.Domain.Master;
using Errordite.Core.Encryption;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.Authentication.Commands
{
    public class ResetPasswordCommand : SessionAccessBase, IResetPasswordCommand
    {
        private readonly IEncryptor _encryptor;
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly IGetOrganisationsByEmailAddressCommand _getOrganisationsByEmailAddressCommand;

        public ResetPasswordCommand(IEncryptor encryptor,
            ISendNotificationCommand sendNotificationCommand, IGetOrganisationsByEmailAddressCommand getOrganisationsByEmailAddressCommand)
        {
            _encryptor = encryptor;
            _sendNotificationCommand = sendNotificationCommand;
            _getOrganisationsByEmailAddressCommand = getOrganisationsByEmailAddressCommand;
        }

        public ResetPasswordResponse Invoke(ResetPasswordRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");

            var response = _getOrganisationsByEmailAddressCommand.Invoke(new GetOrganisationsByEmailAddressRequest
            {
                EmailAddress = request.Email,
            });

            if (response.Organisations == null || response.UserMapping == null)
            {
                return new ResetPasswordResponse
                {
                    Status = ResetPasswordStatus.InvalidEmail,
                };
            }

			var mapping = Session.MasterRaven.Query<UserOrganisationMapping>().FirstOrDefault(m => m.EmailAddress == request.Email);

			if (mapping == null)
			{
				return new ResetPasswordResponse
				{
					Status = ResetPasswordStatus.InvalidEmail
				};
			}

            if (response.Organisations.All(o => o.Status == OrganisationStatus.Suspended))
            {
                return new ResetPasswordResponse
                {
                    Status = ResetPasswordStatus.OrganisationSuspended
                };
            }

            var organisation = response.Organisations.First();
			Session.SetOrganisation(organisation);

			var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email);

			if (user == null)
			{
				return new ResetPasswordResponse
				{
					Status = ResetPasswordStatus.InvalidEmail
				};
			}

            mapping.PasswordToken = Guid.NewGuid();

            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                EmailInfo = new ResetPasswordEmailInfo
                {
                    To = user.Email,
					Token = _encryptor.Encrypt("{0}|{1}".FormatWith(mapping.PasswordToken, user.Email)).Base64Encode(),
                    UserName = user.FirstName
                },
				OrganisationId = organisation.Id
            });

            return new ResetPasswordResponse
            {
                Status = ResetPasswordStatus.Ok
            };
        }
    }

    public interface IResetPasswordCommand : ICommand<ResetPasswordRequest, ResetPasswordResponse>
    { }

    public class ResetPasswordResponse
    {
        public ResetPasswordStatus Status { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
    }

    public enum ResetPasswordStatus
    {
        Ok,
        InvalidEmail,
        OrganisationSuspended
    }
}
