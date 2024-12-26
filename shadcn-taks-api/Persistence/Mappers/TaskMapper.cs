using AutoMapper;
using shadcn_taks_api.Features.Tasks.Dtos;
using shadcn_taks_api.Features.Tasks.Models;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Persistence.Mappers;

public class TaskMapper : Profile
{
    public TaskMapper()
    {
        CreateMap<Task, TaskDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));
        CreateMap<Task, TaskPreloadDto>();
        CreateMap<CreateTaskRequest, Task>()
            .ForMember(desc => desc.Tags, opt => opt.Ignore());
        CreateMap<UpdateTaskRequest, Task>()
            .ForMember(desc => desc.Tags, opt => opt.Ignore());
    }
}