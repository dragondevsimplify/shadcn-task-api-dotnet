using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using Task = shadcn_taks_api.Persistence.Entities.Task;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShadcnTaskDbContext>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


/* ENDPOINTS */

#region TagsEndpoints

app.MapGet("/tags", async ([AsParameters] GetTagListRequest req, ShadcnTaskDbContext dbContext) =>
{
    try
    {
        var tagsTable = dbContext.Tags;
        List<Tag> tags = [];

        if (!string.IsNullOrEmpty(req.SortBy) && !string.IsNullOrEmpty(req.SortOrder.ToString()))
        {
            var sortBy = req.SortBy.ToLower();
            var sortOrder = req.SortOrder.ToString()!.ToUpper();

            var propertyInfo = typeof(Task).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                if (sortOrder == "ASC")
                {
                    tags = await tagsTable.AsNoTracking().OrderBy(task => propertyInfo.GetValue(task, null)).ToListAsync();
                }
                else
                {
                    tags = await tagsTable.AsNoTracking().OrderByDescending(task => propertyInfo.GetValue(task, null)).ToListAsync();
                }
            }
        }

        // Create response data
        var res = tags.Select(i => new TagDto()
        {
            Id = i.Id,
            Name = i.Name,
            Tasks = i.Tasks.Select(t => new TaskPreloadDto()
            {
                Id = t.Id,
                Name = t.Name,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
            }).ToList(),
        });

        return Results.Ok(res);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("GetTags").WithOpenApi();

app.MapPost("/tags", async (CreateTagRequest payload, ShadcnTaskDbContext dbContext) =>
{
    try
    {
        if (payload.Id.HasValue)
        {
            return Results.BadRequest("Id must be null or not set for create tag.");
        }

        var newTag = new Tag()
        {
            Name = payload.Name,
        };

        await dbContext.Tags.AddAsync(newTag);
        await dbContext.SaveChangesAsync();

        // Create response data
        var res = new TagDto()
        {
            Id = newTag.Id,
            Name = newTag.Name,
            Tasks = newTag.Tasks.Select(t => new TaskPreloadDto()
            {
                Id = t.Id,
                Name = t.Name,
                Title = t.Title,
            }).ToList(),
        };

        return Results.Created($"/tags/{res.Id}", res);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("CreateTagDto").WithOpenApi();

#endregion

#region TasksEndpoints

app.MapGet("/tasks", async (ShadcnTaskDbContext dbContext) =>
    {
        try
        {
            var tasks = await dbContext.Tasks.AsNoTracking()
                .Include(i => i.TaskTags)
                .Include(i => i.Tags)
                .ToListAsync();

            var res = tasks.Select(i => new TaskDto()
            {
                Id = i.Id,
                Name = i.Name,
                Title = i.Title,
                Tags = i.Tags.Select(t => new TagPreloadDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
                Status = i.Status,
                Priority = i.Priority,
            });

            return Results.Ok(res);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    })
    .WithName("GetTasks")
    .WithOpenApi();

app.MapPost("/tasks", async (CreateTaskRequest payload, ShadcnTaskDbContext dbContext) =>
    {
        try
        {
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

            // Create response data
            var taskDto = new TaskDto()
            {
                Id = newTask.Id,
                Name = newTask.Name,
                Title = newTask.Title,
                Tags = tags.Select(t => new TagPreloadDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
                Status = newTask.Status,
                Priority = newTask.Priority,
            };

            return Results.Created($"/tasks/{taskDto.Id}", taskDto);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }).WithName("AddTask")
    .WithOpenApi();

#endregion


/* APP RUN */
app.Run();