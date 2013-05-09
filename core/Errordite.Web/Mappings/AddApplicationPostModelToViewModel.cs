
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Web.Models.Applications;

namespace Errordite.Web.Mappings
{
    public class AddApplicationPostModelToViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<AddApplicationPostModel, AddApplicationViewModel>()
                .ForMember(ci => ci.Active, opt => opt.MapFrom(i => i.Active))
                .ForMember(ci => ci.MatchRuleFactoryId, opt => opt.MapFrom(i => i.MatchRuleFactoryId))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name))
                .ForMember(ci => ci.Version, opt => opt.MapFrom(i => i.Version))
                .ForMember(ci => ci.HipChatAuthToken, opt => opt.MapFrom(i => i.HipChatAuthToken))
                .ForMember(ci => ci.HipChatRoomId, opt => opt.MapFrom(i => i.HipChatRoomId));
        }
    }
}