using System;
using CodeTrip.Core;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;
using Errordite.Core.Organisations.Commands;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Authentication.Commands
{
    public class ResetPasswordCommand : SessionAccessBase, IResetPasswordCommand
    {
        private readonly IEncryptor _encryptor;
        private readonly ISendNotificationCommand _sendNotificationCommand;
        private readonly ISetOrganisationByEmailAddressCommand _setOrganisationByEmailAddressCommand;

        public ResetPasswordCommand(IEncryptor encryptor,
            ISendNotificationCommand sendNotificationCommand, ISetOrganisationByEmailAddressCommand setOrganisationByEmailAddressCommand)
        {
            _encryptor = encryptor;
            _sendNotificationCommand = sendNotificationCommand;
            _setOrganisationByEmailAddressCommand = setOrganisationByEmailAddressCommand;
        }

        public ResetPasswordResponse Invoke(ResetPasswordRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Email, "request.Email");

            var organisation = _setOrganisationByEmailAddressCommand.Invoke(new SetOrganisationByEmailAddressRequest()
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

            var user = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.Email == request.Email);

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
