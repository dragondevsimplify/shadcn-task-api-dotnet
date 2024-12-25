using AutoMapper;
using shadcn_taks_api.Features.Tags.Dtos;
using shadcn_taks_api.Persistence.Entities;

namespace shadcn_taks_api.Persistence.Mappers;

public class TagMapper : Profile
{
    public TagMapper()
    {
        CreateMap<Tag, TagDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));
        CreateMap<Tag, TagPreloadDto>();
    }
}