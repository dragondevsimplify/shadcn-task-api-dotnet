using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Extensions;
using shadcn_taks_api.Features.Tags.Models;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Persistence.Entities;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class UpdateTag
{
    public static void MapUpdateTag(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
                int id, UpdateTagRequest tag, ShadcnTaskDbContext dbContext, IMapper mapper) =>
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

                var tagUpdated = mapper.Map<Tag>(tag);
                dbContext.Tags.Update(tagUpdated);
                await dbContext.SaveChangesAsync();

                return TypedResults.NoContent();
            })
            .WithName("UpdateTag")
            .WithOpenApi()
            .WithRequestValidation<UpdateTagRequest>();
    }
}