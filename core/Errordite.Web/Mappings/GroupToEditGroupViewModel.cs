
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Mappings
{
    public class GroupToEditGroupViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<Group, EditGroupViewModel>()
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.FriendlyId))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name));
        }
    }
}