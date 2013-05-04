using System;
using System.Collections.Generic;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Domain.Master;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Encryption;
using Errordite.Core.Interfaces;
using Errordite.Core.Extensions;
using System.Linq;
using Errordite.Core.Session;

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
                return new SetPasswordResponse(true)
                {
                    Status = SetPasswordStatus.InvalidToken
                };
            }

            var token = tokenstring.Split(new [] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            
            Guid userToken;
            if (Guid.TryParse(token[0], out userToken))
            {
	            var email = token[1];
				var mapping = Session.MasterRaven.Query<UserOrganisationMapping>().FirstOrDefault(m => m.EmailAddress == email);

				if (mapping != null)
                {
					if (mapping.PasswordToken != userToken)
                    {
                        return new SetPasswordResponse(true)
                        {
                            Status = SetPasswordStatus.InvalidToken  
                        };
                    }

					mapping.Password = request.Password.Hash();
					mapping.PasswordToken = Guid.Empty;

					if (mapping.Status == UserStatus.Inactive)
					{
						mapping.Status = UserStatus.Active;

						foreach (var organisationId in mapping.Organisations)
						{
							var organisation = Session.MasterRaven
								.Include<Organisation>(o => o.RavenInstanceId)
								.Load<Organisation>(organisationId);

							if (organisation != null)
							{
								organisation.RavenInstance = Session.MasterRaven.Load<RavenInstance>(organisation.RavenInstanceId);

								using (Session.SwitchOrg(organisation))
								{
									var user = Session.Raven.Query<User, Indexing.Users>().FirstOrDefault(u => u.Email == mapping.EmailAddress);

									if (user != null)
									{
										user.Status = UserStatus.Active;
									}
								}
							}
						}
					}

                    return new SetPasswordResponse(false, mapping.EmailAddress, mapping.Organisations)
                    {
                        Status = SetPasswordStatus.Ok
                    };
                }
            }

            return new SetPasswordResponse(true)
            {
                Status = SetPasswordStatus.InvalidToken
            };
        }
    }

    public interface ISetPasswordCommand : ICommand<SetPasswordRequest, SetPasswordResponse>
    { }

    public class SetPasswordResponse : CacheInvalidationResponseBase
    {
	    private readonly string _email;
	    private readonly IEnumerable<string> _organisationIds;

		public SetPasswordResponse(bool ignoreCache = false, string email = null, IEnumerable<string> organisationIds = null)
			: base(ignoreCache)
	    {
		    _email = email;
		    _organisationIds = organisationIds;
	    }

	    public SetPasswordStatus Status { get; set; }

	    protected override IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems()
	    {
		    return _organisationIds.SelectMany(organisationId => Caching.CacheInvalidation.GetUserInvalidationItems(organisationId, _email));
	    }
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
