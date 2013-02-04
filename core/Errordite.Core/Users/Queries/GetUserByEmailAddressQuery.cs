using Castle.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Indexing;
using Errordite.Core.Session;
using System.Linq;
using ProtoBuf;

namespace Errordite.Core.Users.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetUserByEmailAddressQuery : SessionAccessBase, IGetUserByEmailAddressQuery
    {
        public GetUserByEmailAddressResponse Invoke(GetUserByEmailAddressRequest request)
        {
            var user = Query<User, Users_Search>().FirstOrDefault(u => u.Email == request.EmailAddress);

            return new GetUserByEmailAddressResponse()
                {
                    User = user,
                };
        }
    }

    public interface IGetUserByEmailAddressQuery : IQuery<GetUserByEmailAddressRequest, GetUserByEmailAddressResponse>
    { }

    public class GetUserByEmailAddressRequest : CacheableRequestBase<GetUserByEmailAddressResponse>
    {
        public string EmailAddress { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Users.Email(EmailAddress);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Users;
        }
    }

    [ProtoContract]
    public class GetUserByEmailAddressResponse
    {
        [ProtoMember(1)]
        public User User { get; set; }
    }

}