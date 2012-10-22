using System;
using CodeTrip.Core;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using System.Linq;

namespace Errordite.Core.Users.Queries
{
    public interface IGetUserByEmailAddressQuery : IQuery<GetUserByEmailAddressRequest, GetUserByEmailAddressResponse>
    {
    }

    //TODO: caching (maybe)
    public class GetUserByEmailAddressQuery : SessionAccessBase, IGetUserByEmailAddressQuery
    {
        public GetUserByEmailAddressResponse Invoke(GetUserByEmailAddressRequest request)
        {
            var user = Query<User>().FirstOrDefault(u => u.Email == request.EmailAddress);

            return new GetUserByEmailAddressResponse()
                {
                    User = user,
                };
        }
    }

    public class GetUserByEmailAddressRequest
    {
        public string EmailAddress { get; set; }
    }

    public class GetUserByEmailAddressResponse
    {
        public User User { get; set; }
    }

}