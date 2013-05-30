
using AutoMapper;
using Errordite.Core.Interfaces;
using Errordite.Web.Models.Issues;

namespace Errordite.Web.Mappings
{
    public class AddIssuePostModelToViewModel : IMappingDefinition
    {
        public void Define()
        {
            Mapper.CreateMap<AddIssuePostModel, AddIssueViewModel>()
                .ForMember(ci => ci.ApplicationId, opt => opt.MapFrom(i => i.ApplicationId))
                .ForMember(ci => ci.Status, opt => opt.MapFrom(i => i.Status))
                .ForMember(ci => ci.UserId, opt => opt.MapFrom(i => i.UserId))
                .ForMember(ci => ci.Rules, opt => opt.MapFrom(i => i.Rules))
                .ForMember(ci => ci.Name, opt => opt.MapFrom(i => i.Name));
        }
    }
}