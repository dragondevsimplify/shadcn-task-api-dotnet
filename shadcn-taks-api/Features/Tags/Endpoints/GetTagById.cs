using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tags.Dtos;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class GetTabById
{
    public static void MapGetTagById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags/{id:int}", async Task<Results<NotFound, Ok<TagDto>>> (int id, ShadcnTaskDbContext dbContext, IMapper mapper) =>
        {
            var tag = await dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag is null)
            {
                return TypedResults.NotFound();
            }

            var tagDto = mapper.Map<TagDto>(tag);
            return TypedResults.Ok(tagDto);
        }).WithName("GetTagById").WithOpenApi();
    }
}