using AutoMapper;
using shadcn_taks_api.Persistence.Dtos;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Mappers;

public class TaskMapper : Profile
{
    public TaskMapper()
    {
        CreateMap<Task, TaskDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<Task, TaskPreloadDto>();
    }
}