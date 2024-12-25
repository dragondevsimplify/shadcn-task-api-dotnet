using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class UpdateTask
{
    public static void MapUpdateTask(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tasks/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
            int id, UpdateTaskRequest task, ShadcnTaskDbContext dbContext) =>
        {
            // Check task Id
            if (id != task.Id)
            {
                return TypedResults.BadRequest("Id mismatch.");
            }

            // Check task is existing
            var isExist = await dbContext.Tasks.AnyAsync(i => i.Id == id);
            if (!isExist)
            {
                return TypedResults.NotFound();
            }

            // Check task name
            isExist = await dbContext.Tasks.AnyAsync(t => t.Name == task.Name && t.Id != id);
            if (isExist)
            {
                return TypedResults.BadRequest("Task with the same name already exists");
            }

            // Delete task tag
            await dbContext.TaskTags.Where(i => i.TaskId == id).ExecuteDeleteAsync();

            // Create tags if not existing in DB
            List<Tag> tags = [];
            foreach (var payloadTag in task.Tags)
            {
                if (!payloadTag.Id.HasValue)
                {
                    var isExistTag = await dbContext.Tags.AnyAsync(t => t.Name == payloadTag.Name);
                    if (isExistTag)
                    {
                        return TypedResults.BadRequest($"Tag with the same name already exists: {payloadTag.Name}");
                    }

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

            // Update task & save changes
            var taskUpdated = new Task()
            {
                Id = task.Id,
                Name = task.Name,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
            };
            dbContext.Tasks.Update(taskUpdated);
            await dbContext.SaveChangesAsync();

            // Create tasks_tags
            await dbContext.TaskTags.AddRangeAsync(tags.Select(t => new TaskTag()
            {
                TaskId = id,
                TagId = t.Id,
            }));
            await dbContext.SaveChangesAsync();

            return TypedResults.NoContent();
        }).WithName("UpdateTask").WithOpenApi();
    }
}