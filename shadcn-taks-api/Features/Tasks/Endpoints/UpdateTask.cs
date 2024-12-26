using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Features.Tasks.Models;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class UpdateTask
{
    public static void MapUpdateTask(this IEndpointRouteBuilder app)
    {
        app.MapPut("/tasks/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
            int id, UpdateTaskRequest task, ShadcnTaskDbContext dbContext, IMapper mapper) =>
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
            foreach (var newTag in task.Tags.Select(mapper.Map<Tag>))
            {
                if (newTag.Id == 0)
                {
                    var isExistTag = await dbContext.Tags.AnyAsync(t => t.Name == newTag.Name);

                    if (isExistTag)
                    {
                        return TypedResults.BadRequest($"Tag with the same name already exists: {newTag.Name}");
                    }

                    await dbContext.Tags.AddAsync(newTag);
                }

                tags.Add(newTag);
            }

            // Update task & save changes
            var taskUpdated = mapper.Map<Task>(task);
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