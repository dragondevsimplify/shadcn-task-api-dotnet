using AutoMapper;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence.Entities;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Persistence;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tag, TagDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));
        CreateMap<Task, TaskPreloadDto>();

        CreateMap<Task, TaskDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<Tag, TagPreloadDto>();
    }
}