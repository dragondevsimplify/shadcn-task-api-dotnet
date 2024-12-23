using shadcn_taks_api.Persistence.Entities;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Persistence.Dtos;

public class TagDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<TaskPreloadDto> Tasks { get; set; } = [];
}