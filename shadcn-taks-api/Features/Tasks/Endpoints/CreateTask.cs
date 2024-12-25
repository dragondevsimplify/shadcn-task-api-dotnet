using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tasks.Dtos;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Features.Tasks.Models;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class CreateTask
{
    public static void MapCreateTask(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tasks", async Task<Results<BadRequest<string>, CreatedAtRoute<TaskDto>>> (
                CreateTaskRequest payload, ShadcnTaskDbContext dbContext, IMapper mapper) =>
            {
                // Check if name is existing
                var isExist = await dbContext.Tasks.AnyAsync(t => t.Name == payload.Name);
                if (isExist)
                {
                    return TypedResults.BadRequest("Task with the same name already exists");
                }

                // Create tags if not existing in DB
                List<Tag> tags = [];
                foreach (var payloadTag in payload.Tags)
                {
                    if (!payloadTag.Id.HasValue)
                    {
                        var newTag = new Tag()
                        {
                            Name = payloadTag.Name,
                        };
                        await dbContext.Tags.AddAsync(newTag);
                        tags.Add(newTag);
                    }
                    else
                    {
                        tags.Add(new Tag()
                        {
                            Id = payloadTag.Id.Value,
                            Name = payloadTag.Name,
                        });
                    }
                }

                // Create task
                var newTask = new Task()
                {
                    Name = payload.Name,
                    Title = payload.Title,
                    Status = payload.Status,
                    Priority = payload.Priority,
                };
                await dbContext.Tasks.AddAsync(newTask);

                // Save changes
                await dbContext.SaveChangesAsync();

                // Create tasks_tags
                await dbContext.TaskTags.AddRangeAsync(tags.Select(t => new TaskTag()
                {
                    TaskId = newTask.Id,
                    TagId = t.Id,
                }));
                await dbContext.SaveChangesAsync();

                // Assign tags to response
                newTask.Tags = tags;

                // Create response data
                var taskDto = mapper.Map<TaskDto>(newTask);
                return TypedResults.CreatedAtRoute(taskDto, "GetTaskById", new { id = newTask.Id });
            }).WithName("CreateTask")
            .WithOpenApi();
    }
}