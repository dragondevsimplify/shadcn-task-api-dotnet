using AutoMapper;
using shadcn_taks_api.Features.Tasks.Dtos;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Persistence.Mappers;

public class TaskMapper : Profile
{
    public TaskMapper()
    {
        CreateMap<Task, TaskDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<Task, TaskPreloadDto>();
    }
}