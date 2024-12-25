namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class GetTaskStatusList
{
    public static void MapGetTaskStatusList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks/statuses", () =>
        {
            var statuses = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>();
            return TypedResults.Ok(statuses);
        }).WithName("GetTaskStatusList").WithOpenApi();
    }
}