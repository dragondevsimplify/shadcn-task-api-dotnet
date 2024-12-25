using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tasks.Dtos;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class GetTaskById
{
    public static void MapGetTaskById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks/{id:int}", async Task<Results<NotFound, Ok<TaskDto>>> (int id, ShadcnTaskDbContext dbContext, IMapper mapper) =>
        {
            var task = await dbContext.Tasks
                .AsNoTracking()
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task is null)
            {
                return TypedResults.NotFound();
            }

            var taskDto = mapper.Map<TaskDto>(task);
            return TypedResults.Ok(taskDto);
        }).WithName("GetTaskById").WithOpenApi();
    }
}