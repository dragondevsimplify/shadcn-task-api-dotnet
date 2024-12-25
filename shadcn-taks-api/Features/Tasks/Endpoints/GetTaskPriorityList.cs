using shadcn_taks_api.Persistence.Entities;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class GetTaskPriorityList
{
    public static void MapGetTaskPriorityList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks/priorities", () =>
        {
            var priorities = Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>();
            return TypedResults.Ok(priorities);
        }).WithName("GetTaskPriorityList").WithOpenApi();
    }
}