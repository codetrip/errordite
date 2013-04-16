
using AutoMapper;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Mappings
{
    public class GroupToGroupViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<Group, GroupViewModel>()
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.FriendlyId))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name));
        }
    }
}