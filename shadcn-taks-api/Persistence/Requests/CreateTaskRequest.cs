using System.Text.Json.Serialization;
using shadcn_taks_api.Persistence.Entities;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

namespace shadcn_taks_api.Persistence.Requests;

public class CreateTaskRequest
{
    public required string Name { get; set; }
    public required string Title { get; set; }

    public List<CreateTagRequest> Tags { get; set; } = [];

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskStatus Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskPriority Priority { get; set; }
}