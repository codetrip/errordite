
using AutoMapper;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Web.Models.Applications;

namespace Errordite.Web.Mappings
{
    public class ApplicationToEditApplicationViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<Application, EditApplicationViewModel>()
                .ForMember(ci => ci.Id, opt => opt.MapFrom(i => i.FriendlyId))
                .ForMember(ci => ci.IsActive, opt => opt.MapFrom(i => i.IsActive))
                .ForMember(ci => ci.Token, opt => opt.MapFrom(i => i.Token))
                .ForMember(ci => ci.UserId, opt => opt.MapFrom(i => new User{Id = i.DefaultUserId}.FriendlyId))
                .ForMember(ci => ci.Notifications, opt => opt.Ignore())
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name))
                .ForMember(ci => ci.HipChatAuthToken, opt => opt.MapFrom(i => i.HipChatAuthToken))
                .ForMember(ci => ci.HipChatRoomId, opt => opt.MapFrom(i => i.HipChatRoomId));
        }
    }
}