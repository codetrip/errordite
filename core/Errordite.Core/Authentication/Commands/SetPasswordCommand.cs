using System;
using Errordite.Core;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Indexing;
using System.Linq;
using Errordite.Core.Organisations.Queries;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Authentication.Commands
{
    public class SetPasswordCommand : SessionAccessBase, ISetPasswordCommand
    {
        private readonly IGetOrganisationQuery _getOrganisationQuery;
        private readonly IEncryptor _encryptor;

        public SetPasswordCommand(IEncryptor encryptor, IGetOrganisationQuery getOrganisationQuery)
        {
            _encryptor = encryptor;
            _getOrganisationQuery = getOrganisationQuery;
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

            var token = tokenstring.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            
            Guid userToken;
            if (Guid.TryParse(token[0], out userToken))
            {
                var organisation = _getOrganisationQuery.Invoke(new GetOrganisationRequest
                {
                    OrganisationId = token[1]
                }).Organisation;

                if(organisation != null)
                {
                    Session.SetOrganisation(organisation);

                    var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.PasswordToken == userToken);

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
