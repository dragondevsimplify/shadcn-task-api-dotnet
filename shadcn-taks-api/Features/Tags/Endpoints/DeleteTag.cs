using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class DeleteTag
{
    public static void MapDeleteTag(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/tags/{id:int}", async Task<Results<NotFound, NoContent>> (int id, ShadcnTaskDbContext dbContext) =>
        {
            var deletedCount = await dbContext.Tags.Where(i => i.Id == id).ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.NoContent();
        }).WithName("DeleteTag").WithOpenApi();
    }
}