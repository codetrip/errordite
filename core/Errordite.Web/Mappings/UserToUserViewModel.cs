
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Web.Models.Users;
using System.Linq;

namespace Errordite.Web.Mappings
{
    public class UserToUserViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<User, UserViewModel>()
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.FriendlyId))
                .ForMember(ci => ci.Status, opt => opt.MapFrom(i => i.Status))
                .ForMember(ci => ci.Role, opt => opt.MapFrom(i => i.Role))
                .ForMember(ci => ci.Email, opt => opt.MapFrom(i => i.Email))
                .ForMember(ci => ci.Groups, opt => opt.MapFrom(i => i.Groups != null ? i.Groups.Aggregate(string.Empty, (current, t) => current + (t.Name + ',')).TrimEnd(',') : string.Empty))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => "{0} {1}".FormatWith(i.FirstName, i.LastName)));
        }
    }
}