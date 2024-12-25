using shadcn_taks_api.Persistence.Entities;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

namespace shadcn_taks_api.Features.Tasks.Dtos;

// Preload <=> Use Include in LINQ
public class TaskPreloadDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Title { get; set; }

    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
}