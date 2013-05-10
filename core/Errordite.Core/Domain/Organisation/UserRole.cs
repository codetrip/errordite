
using ProtoBuf;

namespace Errordite.Core.Domain.Organisation
{
    [ProtoContract]
    public enum UserRole
    {
        [ProtoMember(1)]
        Guest, //Not registered
        [ProtoMember(2)]
        User, //can manage errors & classes but not the organisation
        [ProtoMember(3)]
        Administrator, //Can manage an organisation (users, billing, applications etc)
        [ProtoMember(4)]
        SuperUser //Reserved for Gaz & Nick!
    }
}