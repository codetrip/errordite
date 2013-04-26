using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using ProtoBuf;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Groups.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetGroupQuery : SessionAccessBase, IGetGroupQuery
    {
        private readonly IAuthorisationManager _authorisationManager;

        public GetGroupQuery(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public GetGroupResponse Invoke(GetGroupRequest request)
        {
            Trace("Starting...");

            var groupId = Group.GetId(request.GroupId);
            var group = Load<Group>(groupId);

            if(group != null)
            {
                _authorisationManager.Authorise(group, request.CurrentUser);
            }

            return new GetGroupResponse
            {
                Group = group
            };
        }
    }

    public interface IGetGroupQuery : IQuery<GetGroupRequest, GetGroupResponse>
    { }

    [ProtoContract]
    public class GetGroupResponse
    {
        [ProtoMember(1)]
        public Group Group { get; set; }
    }

    public class GetGroupRequest : CacheableOrganisationRequestBase<GetGroupResponse>
    {
        public string GroupId { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Groups.Key(CurrentUser.OrganisationId, GroupId);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Groups;
        }
    }
}
