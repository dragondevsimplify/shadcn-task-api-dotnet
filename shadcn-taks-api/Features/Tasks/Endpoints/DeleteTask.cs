using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class DeleteTask
{
    public static void MapDeleteTask(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/tasks/{id:int}", async Task<Results<NotFound, NoContent>> (int id, ShadcnTaskDbContext dbContext) =>
        {
            var deletedCount = await dbContext.Tasks.Where(i => i.Id == id).ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.NoContent();
        }).WithName("DeleteTask").WithOpenApi();
    }
}