using shadcn_taks_api.Persistence.Entities;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

namespace shadcn_taks_api.Persistence.Dtos;

public class TaskPreloadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
}