using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Extensions;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tags.Dtos;
using shadcn_taks_api.Features.Tags.Models;
using shadcn_taks_api.Filters;
using shadcn_taks_api.Persistence.Entities;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class CreateTag
{
    public static void MapCreateTag(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tags",
                async Task<Results<BadRequest<string>, CreatedAtRoute<TagDto>>> (CreateTagRequest tag,
                    ShadcnTaskDbContext dbContext, IMapper mapper) =>
                {
                    var isExist = await dbContext.Tags.AnyAsync(t => t.Name == tag.Name);
                    if (isExist)
                    {
                        return TypedResults.BadRequest("Name is already in use.");
                    }

                    var newTag = mapper.Map<Tag>(tag);
                    await dbContext.Tags.AddAsync(newTag);
                    await dbContext.SaveChangesAsync();

                    // Create response data
                    var tagDto = mapper.Map<TagDto>(newTag);
                    return TypedResults.CreatedAtRoute(tagDto, "GetTagById", new { id = newTag.Id });
                })
            .WithName("CreateTag")
            .WithOpenApi()
            .WithRequestValidation<CreateTagRequest>();
    }
}