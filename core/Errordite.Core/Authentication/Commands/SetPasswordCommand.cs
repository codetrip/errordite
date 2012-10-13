using System;
using CodeTrip.Core;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using CodeTrip.Core.Extensions;
using Errordite.Core.Indexing;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Authentication.Commands
{
    public class SetPasswordCommand : SessionAccessBase, ISetPasswordCommand
    {
        private readonly IEncryptor _encryptor;

        public SetPasswordCommand(IEncryptor encryptor)
        {
            _encryptor = encryptor;
        }

        public SetPasswordResponse Invoke(SetPasswordRequest request)
        {
            Trace("Starting...");

            ArgumentValidation.NotEmpty(request.Token, "request.Token");
            ArgumentValidation.NotEmpty(request.Password, "request.Password");

            string tokenstring;
            try
            {
                tokenstring = _encryptor.Decrypt(request.Token.Base64Decode());
            }
            catch
            {
                return new SetPasswordResponse
                {
                    Status = SetPasswordStatus.InvalidToken
                };
            }
            
            Guid token;
            if (Guid.TryParse(tokenstring, out token))
            {
                var user = Session.Raven.Query<User, Users_Search>().FirstOrDefault(u => u.PasswordToken == token);

                if (user == null || user.PasswordToken == default(Guid))
                {
                    return new SetPasswordResponse
                    {
                        Status = SetPasswordStatus.InvalidToken  
                    };
                }

                user.Password = request.Password.Hash();
                user.PasswordToken = Guid.Empty;
                user.Status = UserStatus.Active;

                Store(user);

                return new SetPasswordResponse
                {
                    Status = SetPasswordStatus.Ok
                };
            }

            return new SetPasswordResponse
            {
                Status = SetPasswordStatus.InvalidToken
            };
        }
    }

    public interface ISetPasswordCommand : ICommand<SetPasswordRequest, SetPasswordResponse>
    { }

    public class SetPasswordResponse
    {
        public SetPasswordStatus Status { get; set; }
    }

    public class SetPasswordRequest
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }

    public enum SetPasswordStatus
    {
        Ok,
        InvalidToken
    }
}
