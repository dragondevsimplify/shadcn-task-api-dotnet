using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Features.Tags.Models;
using shadcn_taks_api.Persistence.Contexts;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class UpdateTag
{
    public static void MapUpdateTag(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
            int id, UpdateTagRequest tag, ShadcnTaskDbContext dbContext) =>
        {
            if (id != tag.Id)
            {
                return TypedResults.BadRequest("Id mismatch.");
            }

            var isExist = await dbContext.Tags.AnyAsync(i => i.Id == id);
            if (!isExist)
            {
                return TypedResults.NotFound();
            }

            isExist = await dbContext.Tags.AnyAsync(t => t.Name == tag.Name && t.Id != tag.Id);
            if (isExist)
            {
                return TypedResults.BadRequest("Name is already in use.");
            }

            dbContext.Update(tag);
            await dbContext.SaveChangesAsync();

            return TypedResults.NoContent();
        }).WithName("UpdateTag").WithOpenApi();
    }
}