
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Applications;

namespace Errordite.Web.Mappings
{
    public class ApplicationToApplicationViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<Application, ApplicationViewModel>()
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.FriendlyId))
                .ForMember(ci => ci.RuleMatchFactory, opt => opt.MapFrom(i => i.MatchRuleFactoryId))
                .ForMember(ci => ci.IsActive, opt => opt.MapFrom(i => i.IsActive))
                .ForMember(ci => ci.Token, opt => opt.MapFrom(i => i.Token))
                .ForMember(ci => ci.DefaultUserId, opt => opt.MapFrom(i => i.DefaultUserId))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name))
				.ForMember(ci => ci.Version, opt => opt.MapFrom(i => i.Version))
				.ForMember(ci => ci.HipChatRoomId, opt => opt.MapFrom(i => i.HipChatRoomId))
				.ForMember(ci => ci.CampfireRoomId, opt => opt.MapFrom(i => i.CampfireRoomId));
        }
    }
}