using System;
using Errordite.Core.Encryption;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
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
        private readonly IGetOrganisationByEmailAddressCommand _getOrganisationByEmailAddressCommand;

        public ResetPasswordCommand(IEncryptor encryptor,
            ISendNotificationCommand sendNotificationCommand, IGetOrganisationByEmailAddressCommand getOrganisationByEmailAddressCommand)
        {
            _encryptor = encryptor;
            _sendNotificationCommand = sendNotificationCommand;
            _getOrganisationByEmailAddressCommand = getOrganisationByEmailAddressCommand;
        }

        public ResetPasswordResponse Invoke(ResetPasswordRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");

            var organisation = _getOrganisationByEmailAddressCommand.Invoke(new GetOrganisationByEmailAddressRequest
            {
                EmailAddress = request.Email,
            }).Organisation;

            if (organisation == null)
            {
                return new ResetPasswordResponse()
                {
                    Status = ResetPasswordStatus.InvalidEmail,
                };
            }

            Session.SetOrganisation(organisation);

            var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return new ResetPasswordResponse
                {
                    Status = ResetPasswordStatus.InvalidEmail  
                };
            }

            user.PasswordToken = Guid.NewGuid();

            _sendNotificationCommand.Invoke(new SendNotificationRequest
            {
                EmailInfo = new ResetPasswordEmailInfo
                {
                    To = user.Email,
                    Token = _encryptor.Encrypt(user.PasswordToken.ToString()).Base64Encode(),
                    UserName = user.FirstName
                },
                OrganisationId = user.OrganisationId
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
        InvalidEmail
    }
}
