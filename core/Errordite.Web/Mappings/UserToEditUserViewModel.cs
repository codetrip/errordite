
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Users;

namespace Errordite.Web.Mappings
{
    public class UserToEditUserViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<User, EditUserViewModel>()
                .ForMember(ci => ci.Email, opt => opt.MapFrom(i => i.Email))
                .ForMember(ci => ci.FirstName, opt => opt.MapFrom(i => i.FirstName))
                .ForMember(ci => ci.LastName, opt => opt.MapFrom(i => i.LastName));
        }
    }
}