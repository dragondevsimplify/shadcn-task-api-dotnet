using shadcn_taks_api.Models;
using shadcn_taks_api.Persistence.Entities;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

namespace shadcn_taks_api.Features.Tasks.Models;

public class GetTaskListRequest : PaginationParams
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public int[]? TagIds { get; set; }
    public TaskStatus[]? Statuses { get; set; }
    public TaskPriority[]? Priorities { get; set; }
}