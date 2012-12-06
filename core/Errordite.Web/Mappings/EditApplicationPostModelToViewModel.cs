
using AutoMapper;
using CodeTrip.Core.Interfaces;
using Errordite.Web.Models.Applications;

namespace Errordite.Web.Mappings
{
    public class EditApplicationPostModelToViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<EditApplicationPostModel, EditApplicationViewModel>()
                .ForMember(ci => ci.IsActive, opt => opt.MapFrom(i => i.IsActive))
                .ForMember(ci => ci.Token, opt => opt.MapFrom(i => i.Token))
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.Id))
                .ForMember(ci => ci.MatchRuleFactoryId, opt => opt.MapFrom(i => i.MatchRuleFactoryId))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name))
                .ForMember(ci => ci.HipChatAuthToken, opt => opt.MapFrom(i => i.HipChatAuthToken))
                .ForMember(ci => ci.HipChatRoomId, opt => opt.MapFrom(i => i.HipChatRoomId));
        }
    }
}